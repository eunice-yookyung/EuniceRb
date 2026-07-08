function res = demo_lattice_registration(doPlot)
%DEMO_LATTICE_REGISTRATION  Register two lattice images: phase + disorder fingerprint.
%
%   res = demo_lattice_registration        % with plots
%   res = demo_lattice_registration(false) % headless
%
%   Builds a synthetic pair of noisy square-lattice images that share a fixed
%   site-to-site "disorder fingerprint" (tied to the lattice), but differ by
%     - a lattice shift of several (non-integer) lattice sites,
%     - an independent drift of a slowly varying incommensurate envelope,
%     - a global intensity change and additive noise,
%   then recovers the shift and tracks one unique physical site in both images.
%
%   The reusable part is REGISTER_LATTICE_PAIR (local function below):
%       res = register_lattice_pair(im1, im2 [, opts])
%   Pipeline:
%     1) Lattice wavevectors k1,k2 from the Bragg peaks of the FFT of im1
%        (coarse peak pick, then sub-bin refinement by maximizing the
%        demodulation amplitude).  Assumes both images share the same
%        lattice spacing/orientation (only phase differs).
%     2) Per image: flat-field (divide by a heavily smoothed copy of itself,
%        removing the multiplicative envelope), then demodulate at k1,k2
%        with a Hann window and centered coordinates -> lattice phase ->
%        sub-pixel grid origin O (a lattice site near the image center).
%     3) Per-site amplitudes sampled on the integer lattice grid
%        (Gaussian-smoothed flat-fielded image, interpolated at site
%        positions), then LOCALLY normalized over a few-site neighborhood.
%        This removes residual envelope drift and global intensity, leaving
%        the disorder fingerprint.
%     4) Integer site offset d between the two grids from normalized
%        cross-correlation of the fingerprints.  Discriminability is
%        reported as peak vs. runner-up correlation.
%     5) Total lattice shift in camera pixels, exact incl. the fractional
%        part:  pixelShift = (O2 - O1) + L*d,   L = [a1 a2].
%        A site with index s in image 1 sits at  O2 + L*(s + d)  in image 2.
%
%   Notes / assumptions:
%     - Square lattice (k2 is picked with |k2| ~ |k1|, non-collinear).
%     - If the pattern is trough-centered rather than peak-centered, O lands
%       on minima instead of maxima -- consistently in both images, so the
%       registration is unaffected.
%     - No toolboxes required (base MATLAB; also runs in Octave).

if nargin < 1, doPlot = true; end
rng(7);

%% ------------------------- synthetic test pair -------------------------
N     = 512;                 % image size (px)
aLat  = 7.3;                 % lattice period (px)
th    = 4*pi/180;            % lattice angle
L_true = aLat*[cos(th) -sin(th); sin(th) cos(th)];   % columns = a1, a2

Rs = ceil(0.8*N/aLat);
[NNg, MMg] = ndgrid(-Rs:Rs);
Adis = 1 + 0.12*randn(size(NNg));      % fixed disorder, tied to lattice indices

trueShiftSites = [3.42; -2.63];        % lattice shift im1 -> im2 (lattice units)
envShift       = [11.7; -6.2];         % independent envelope drift (px)

im1 = make_image(N, L_true, NNg, MMg, Adis, [0;0],          [0;0],    1.00, 0.30*aLat, 0.03);
im2 = make_image(N, L_true, NNg, MMg, Adis, trueShiftSites, envShift, 0.85, 0.30*aLat, 0.03);

%% ------------------------------ register ------------------------------
res = register_lattice_pair(im1, im2);

pixTrue  = L_true*trueShiftSites;
errPix   = res.pixelShift - pixTrue;
errSites = norm(res.L \ errPix);

fprintf('--- lattice registration demo ---\n');
fprintf('lattice const (fit): %.4f px  (true %.4f px)\n', res.aMean, aLat);
fprintf('integer site offset d = [%d, %d]\n', res.dInt(1), res.dInt(2));
fprintf('recovered shift : [%8.3f, %8.3f] px\n', res.pixelShift);
fprintf('true shift      : [%8.3f, %8.3f] px\n', pixTrue);
fprintf('error           : %.4f lattice sites\n', errSites);
fprintf('fingerprint corr peak %.3f, runner-up %.3f (margin %.3f)\n', ...
        res.ccPeak, res.ccRunnerUp, res.ccPeak - res.ccRunnerUp);
if errSites < 0.1, fprintf('PASS: shift recovered to <0.1 site.\n');
else,              fprintf('WARNING: error exceeds 0.1 site.\n'); end

%% ----------------------- track one unique site ------------------------
s1 = [2; -3];                          % site index in image-1 grid
p1 = res.grid1.O + res.L*s1;           % its camera position in im1
p2 = res.grid2.O + res.L*(s1 + res.dInt);   % same physical site in im2

if doPlot
    figure('Name','lattice registration');
    ax1 = subplot(1,2,1); imagesc(im1); axis image; colormap gray; hold on;
    plot(p1(1), p1(2), 'r+', 'MarkerSize', 16, 'LineWidth', 2);
    title('im1: tracked site');
    ax2 = subplot(1,2,2); imagesc(im2); axis image; hold on;
    plot(p2(1), p2(2), 'r+', 'MarkerSize', 16, 'LineWidth', 2);
    title('im2: same physical site');
    linkaxes([ax1, ax2], 'xy')

    figure('Name','fingerprint cross-correlation');
    imagesc(res.shiftRange, res.shiftRange, res.ccMap); axis image;
    xlabel('dm (sites)'); ylabel('dn (sites)'); colorbar;
    title('fingerprint NCC vs. integer offset');
end
if nargout == 0, clear res; end
end

%% ======================================================================
function res = register_lattice_pair(im1, im2, opts)
%REGISTER_LATTICE_PAIR  Shift between two lattice images via phase + fingerprint.
if nargin < 3, opts = struct(); end
opts = set_default(opts, 'maxShift',         10);    % NCC search range (sites)
opts = set_default(opts, 'envSigmaSites',     4);    % flat-field smoothing (in a)
opts = set_default(opts, 'sampleSigmaSites', 0.25);  % site-sampling smoothing (in a)
opts = set_default(opts, 'normHalf',          3);    % local-norm half-window (sites)
opts = set_default(opts, 'margin',            4);    % edge margin (px)
opts = set_default(opts, 'minOverlap',       50);    % min sites for a valid NCC point

% 1) common lattice wavevectors from image 1 (spacing/orientation assumed shared)
[k1, k2] = find_lattice_k(im1);
K = [k1.'; k2.'];
L = inv(K);                            % columns a1,a2:  k_i . a_j = delta_ij
aMean = mean(sqrt(sum(L.^2, 1)));

% 2)+3) per-image grid origin from phase, per-site fingerprint
g1 = analyze_image(im1, k1, k2, L, aMean, opts);
g2 = analyze_image(im2, k1, k2, L, aMean, opts);

% 4) integer offset between the two site grids from the disorder fingerprint
[dInt, ccPeak, ccRunnerUp, ccMap, shiftRange] = ...
    fingerprint_offset(g1.Anorm, g2.Anorm, opts.maxShift, opts.minOverlap);

% 5) total shift in camera pixels (fraction from phase, integer from fingerprint)
res.k1 = k1;  res.k2 = k2;  res.L = L;  res.aMean = aMean;
res.grid1 = g1;  res.grid2 = g2;
res.dInt = dInt;
res.pixelShift = (g2.O - g1.O) + L*dInt;
res.ccPeak = ccPeak;  res.ccRunnerUp = ccRunnerUp;
res.ccMap = ccMap;    res.shiftRange = shiftRange;
end

%% ======================================================================
function g = analyze_image(im, k1, k2, L, aMean, opts)
%ANALYZE_IMAGE  Flat-field, lattice phase -> grid origin, per-site fingerprint.
[Ny, Nx] = size(im);
im = double(im);

% flat-field: divide out the slowly varying multiplicative envelope
sm  = gauss_smooth(im, opts.envSigmaSites*aMean);
imf = im ./ max(sm, 1e-9*max(sm(:)));

% Hann window, centered coordinates (kills first-order bias from small k error)
wx = hannv(Nx);  wy = hannv(Ny);  W = wy*wx.';
cx = (Nx+1)/2;   cy = (Ny+1)/2;
xc = (1:Nx) - cx;   yc = (1:Ny) - cy;
mu  = sum(sum(W.*imf))/sum(W(:));
Wim = W.*(imf - mu);

% demodulate at the exact lattice wavevectors -> phases -> fractional origin
C1 = demod2(Wim, xc, yc, k1);
C2 = demod2(Wim, xc, yc, k2);
u  = mod(-[angle(C1); angle(C2)]/(2*pi), 1);   % k_j . r0 mod 1
r0 = [k1.'; k2.'] \ u;                         % offset from center, mod lattice
O  = [cx; cy] + r0 - L*round(L\r0);            % a lattice site near image center

% per-site amplitudes on the integer grid
R = ceil(0.75*max(Nx, Ny)/aMean);
[nI, mI] = ndgrid(-R:R);                       % n along rows, m along cols
P = O + L*[nI(:).'; mI(:).'];
ims = gauss_smooth(imf, opts.sampleSigmaSites*aMean);
[X, Y] = meshgrid(1:Nx, 1:Ny);
v = interp2(X, Y, ims, P(1,:), P(2,:), 'linear', NaN);
bad = P(1,:) < 1+opts.margin | P(1,:) > Nx-opts.margin | ...
      P(2,:) < 1+opts.margin | P(2,:) > Ny-opts.margin;
v(bad) = NaN;
A = reshape(v, size(nI));

g.O = O;  g.L = L;  g.R = R;
g.A = A;
g.Anorm = local_norm(A, opts.normHalf);        % the disorder fingerprint
end

%% ======================================================================
function [k1, k2] = find_lattice_k(im)
%FIND_LATTICE_K  Coarse Bragg-peak pick + sub-bin refinement (square lattice).
im = double(im);
[Ny, Nx] = size(im);
wx = hannv(Nx);  wy = hannv(Ny);  W = wy*wx.';
im0 = im - mean(im(:));
Wim = W.*im0;

F   = fftshift(fft2(Wim));
mag = abs(F);
fx  = ((0:Nx-1) - floor(Nx/2))/Nx;
fy  = ((0:Ny-1) - floor(Ny/2))/Ny;
[FX, FY] = meshgrid(fx, fy);

% exclude DC neighborhood and keep one half-plane (conjugate pair -> one peak)
mag(FX.^2 + FY.^2 < (6/min(Nx,Ny))^2) = 0;
mag(FY < 0 | (FY == 0 & FX < 0)) = 0;

[~, i1] = max(mag(:));
k1c = [FX(i1); FY(i1)];

% second peak: non-collinear (rejects harmonics) and similar |k| (square lattice)
n1  = k1c/norm(k1c);
kk  = sqrt(FX.^2 + FY.^2);
sina = abs(FX*n1(2) - FY*n1(1)) ./ max(kk, eps);
mag2 = mag;
mag2(sina < 0.5 | abs(kk - norm(k1c)) > 0.35*norm(k1c)) = 0;
[~, i2] = max(mag2(:));
k2c = [FX(i2); FY(i2)];

% refine to sub-bin precision by maximizing the demodulation amplitude
xc = (1:Nx) - (Nx+1)/2;   yc = (1:Ny) - (Ny+1)/2;
k1 = refine_k(Wim, xc, yc, k1c, 1/min(Nx,Ny));
k2 = refine_k(Wim, xc, yc, k2c, 1/min(Nx,Ny));
end

function k = refine_k(Wim, xc, yc, k0, binsz)
obj  = @(k) -abs(demod2(Wim, xc, yc, k(:)));
o    = optimset('TolX', 1e-4*binsz, 'TolFun', 1e-9, ...
                'MaxFunEvals', 300, 'MaxIter', 300, 'Display', 'off');
k = fminsearch(obj, k0, o);
k = k(:);
end

function C = demod2(Wim, xc, yc, k)
% C = sum_{x,y} Wim(y,x) * exp(-2i*pi*(k(1)*xc(x) + k(2)*yc(y)))
ex = exp(-2i*pi*k(1)*xc(:));
ey = exp(-2i*pi*k(2)*yc(:));
C  = ey.' * Wim * ex;
end

%% ======================================================================
function [d, ccPeak, ccRunnerUp, cc, shiftRange] = fingerprint_offset(A1, A2, maxShift, minOverlap)
%FINGERPRINT_OFFSET  Integer grid offset d with A2(i+d) ~ A1(i), via NCC.
S  = size(A1, 1);
sh = -maxShift:maxShift;
cc = -Inf(numel(sh));
for a = 1:numel(sh)
    dn = sh(a);
    r1 = max(1, 1-dn):min(S, S-dn);   r2 = r1 + dn;
    for b = 1:numel(sh)
        dm = sh(b);
        c1 = max(1, 1-dm):min(S, S-dm);   c2 = c1 + dm;
        B1 = A1(r1, c1);  B2 = A2(r2, c2);
        m  = isfinite(B1) & isfinite(B2);
        if nnz(m) < minOverlap, continue; end
        v1 = B1(m);  v2 = B2(m);
        v1 = v1 - mean(v1);  v2 = v2 - mean(v2);
        den = norm(v1)*norm(v2);
        if den > 0, cc(a, b) = (v1.'*v2)/den; end
    end
end
[ccPeak, imax] = max(cc(:));
[ia, ib] = ind2sub(size(cc), imax);
d = [sh(ia); sh(ib)];

tmp = cc;
tmp(max(1, ia-1):min(end, ia+1), max(1, ib-1):min(end, ib+1)) = -Inf;
ccRunnerUp = max(tmp(:));
shiftRange = sh;
end

%% ======================================================================
function An = local_norm(A, h)
%LOCAL_NORM  (A - local mean)/local std over a (2h+1)^2 site window, NaN-aware.
M  = isfinite(A);
A0 = A;  A0(~M) = 0;
K  = ones(2*h+1);
cnt = conv2(double(M), K, 'same');
mu  = conv2(A0,        K, 'same') ./ max(cnt, 1);
va  = conv2(A0.^2,     K, 'same') ./ max(cnt, 1) - mu.^2;
sd  = sqrt(max(va, 0));
An  = (A - mu) ./ max(sd, 1e-12);
An(~M | cnt < 0.4*numel(K)) = NaN;
end

function sm = gauss_smooth(im, sigma)
%GAUSS_SMOOTH  Separable Gaussian smoothing with edge correction (no toolboxes).
r = ceil(3*sigma);
g = exp(-(-r:r).^2/(2*sigma^2));  g = g/sum(g);
num = conv2(g.', g, im,             'same');
den = conv2(g.', g, ones(size(im)), 'same');
sm  = num ./ den;
end

function w = hannv(n)
w = 0.5*(1 - cos(2*pi*(0:n-1).'/(n-1)));
end

function s = set_default(s, f, v)
if ~isfield(s, f) || isempty(s.(f)), s.(f) = v; end
end

%% ======================================================================
function im = make_image(N, L, NN, MM, Adis, shiftSites, envShift, gain, psfSigma, noiseRel)
%MAKE_IMAGE  Synthetic lattice image: fixed disorder tied to lattice indices,
% shifted lattice, independently drifting incommensurate envelope, noise.
c0 = [(N+1)/2; (N+1)/2];
pos = c0 + L*([NN(:).'; MM(:).'] + shiftSites(:));
amp = Adis(:).';
keep = pos(1,:) >= 2 & pos(1,:) <= N-1 & pos(2,:) >= 2 & pos(2,:) <= N-1;
x = pos(1, keep).';  y = pos(2, keep).';  a = amp(keep).';

% bilinear splat of site amplitudes, then blur with the PSF
x0 = floor(x);  y0 = floor(y);  fx = x - x0;  fy = y - y0;
idx = [y0 + (x0-1)*N;  y0 + x0*N;  y0+1 + (x0-1)*N;  y0+1 + x0*N];
w   = [a.*(1-fx).*(1-fy);  a.*fx.*(1-fy);  a.*(1-fx).*fy;  a.*fx.*fy];
D = reshape(accumarray(idx, w, [N*N, 1]), N, N);

r = ceil(3*psfSigma);
g = exp(-(-r:r).^2/(2*psfSigma^2));
spots = conv2(g.', g, D, 'same');

[X, Y] = meshgrid(1:N, 1:N);
ex = X - c0(1) - envShift(1);
ey = Y - c0(2) - envShift(2);
env = 0.55 + 0.45*exp(-(ex.^2 + ey.^2)/(2*(0.35*N)^2));
env = env .* (1 + 0.10*(X - c0(1))/N);

im = gain*(env.*spots);
im = im + noiseRel*max(im(:))*randn(N);
end