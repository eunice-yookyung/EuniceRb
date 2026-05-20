% Demo script: generate a fake Zernike phase map and fit it
% Assumes you already have these functions on your MATLAB path:
%  fitZernike.m
%  zernikePolynomial.m
%  zernikeRadial.m

clear; close all; clc;

% Parameters
Nx = 256;
Ny = 256;

nOrderOriginal = 3;   % highest radial order used to generate fake phase
nOrderFit = nOrderOriginal;        % highest radial order used for fitting

cx = Nx/2;
cy = Ny/2;
radius = Nx/2 * .9;

center = [cy cx];

noiseStd = .01;

% Coordinate grid
x_ = 1:Nx;
y_ = 1:Ny;
[X_, Y_] = meshgrid(x_, y_);

X = (X_ - center(2)) / radius;
Y = (Y_ - center(1)) / radius;
x = (x_ - center(2)) / radius;
y = (y_ - center(1)) / radius;

rho = sqrt(X.^2 + Y.^2);
theta = atan2(Y, X);

pupilMask = rho <= 1;

% Make a fake phase map from random known Zernike terms
DPhaseTrue = zeros(Ny, Nx);
phaseTrue = zeros(Ny, Nx); % zernike phase map, original

% Choose all true modes up to nOrderOriginal
trueModes = [];

for n = 0:nOrderOriginal
    for m = -n:2:n
        trueModes = [trueModes; n, m]; %#ok<AGROW>
    end
end

% Random coefficients with higher radial orders attenuated
coeffScale = 0.5;

attenuationPower = 2;  % larger value suppresses high orders more strongly

radialOrders = trueModes(:, 1);

orderWeights = 1 ./ (radialOrders + 1).^attenuationPower;

randomCoeffs = coeffScale * orderWeights .* randn(size(trueModes, 1), 1);
% Alternative: uniform random coefficients in [-coeffScale, +coeffScale]
% randomCoeffs = coeffScale * (2 * rand(size(trueModes, 1), 1) - 1);

% Each row is [n, m, coefficient]
trueTerms = [trueModes, randomCoeffs];

for k = 1:size(trueTerms, 1)
    n = trueTerms(k, 1);
    m = trueTerms(k, 2);
    c = trueTerms(k, 3);

    [Zx, Zy] = zernikePolynomialDerivative(n, m, rho, theta, true);
    Z = zernikePolynomialDerivative(n, m, rho, theta, true);
    DPhaseTrue = DPhaseTrue + c * Zx;
    phaseTrue = phaseTrue + c * Z;
end

% Mask outside the aperture
DPhaseTrue(~pupilMask) = NaN;
phaseTrue(~pupilMask) = NaN;

% Add optional noise
% rng(1);

DPhaseNoisy = DPhaseTrue;
DPhaseNoisy(pupilMask) = DPhaseNoisy(pupilMask) + noiseStd * randn(nnz(pupilMask), 1);

% Fit Zernike coefficients
[coeffs, modes, DPhaseFit, residual, fitMask] = fitZernikeDerivative( ...
    DPhaseNoisy, ...
    nOrderFit, ...
    'Mask', pupilMask, ...
    'Center', center, ...
    'Radius', radius, ...
    'Normalize', true);

fittedTerms = trueTerms;
fittedTerms(:, 3) = coeffs;
phaseFit = 0;
for k = 1:size(fittedTerms, 1)
    n = fittedTerms(k, 1);
    m = fittedTerms(k, 2);
    c = fittedTerms(k, 3);

    Z = zernikePolynomial(n, m, rho, theta, true);
    phaseFit = phaseFit + c * Z;
end
phaseFit(~pupilMask) = NaN;

% Print original and recovered coefficients side by side
fprintf('\nZernike coefficient comparison:\n');
fprintf('----------------------------------------------------\n');
fprintf('%5s %5s %14s %14s %14s\n', ...
    'n', 'm', 'true coeff', 'fit coeff', 'error');
fprintf('----------------------------------------------------\n');

for j = 1:size(modes, 1)
    n = modes(j, 1);
    m = modes(j, 2);

    trueCoeff = 0;

    idx = trueTerms(:, 1) == n & trueTerms(:, 2) == m;

    if any(idx)
        trueCoeff = trueTerms(idx, 3);
    end

    fitCoeff = coeffs(j);
    coeffError = fitCoeff - trueCoeff;

    fprintf('%5d %5d %14.6f %14.6f %14.6f\n', ...
        n, m, trueCoeff, fitCoeff, coeffError);
end

fprintf('----------------------------------------------------\n');

% Prepare coefficient arrays for plotting
trueCoeffs = zeros(size(coeffs));

for j = 1:size(modes, 1)
    n = modes(j, 1);
    m = modes(j, 2);

    idx = trueTerms(:, 1) == n & trueTerms(:, 2) == m;

    if any(idx)
        trueCoeffs(j) = trueTerms(idx, 3);
    end
end

coeffError = trueCoeffs - coeffs;

modeLabels = strings(size(modes, 1), 1);

for j = 1:size(modes, 1)
    modeLabels(j) = sprintf('(%d,%+d)', modes(j, 1), modes(j, 2));
end

% Choose cut positions
x0_cut = -.3;
y0_cut = -.3;

% Find nearest grid indices
[~, ix_cut] = min(abs(x - x0_cut));
[~, iy_cut] = min(abs(y - y0_cut));

% Extract horizontal cuts: y = y0_cut
phase_true_xcut = DPhaseTrue(iy_cut, :);
phase_fit_xcut  = DPhaseFit(iy_cut, :);

% Extract vertical cuts: x = x0_cut
phase_true_ycut = DPhaseTrue(:, ix_cut);
phase_fit_ycut  = DPhaseFit(:, ix_cut);

% Pixel/grid spacing in normalized pupil coordinates
dx = 1 / radius;
dy = 1 / radius;

% Derivatives of true and fitted phase maps
[dphidx_true, dphidy_true] = gradient(DPhaseTrue, dx, dy);
[dphidx_fit,  dphidy_fit]  = gradient(DPhaseFit,  dx, dy);

% Mask outside the aperture
dphidx_true(~pupilMask) = NaN;
dphidy_true(~pupilMask) = NaN;

dphidx_fit(~pupilMask) = NaN;
dphidy_fit(~pupilMask) = NaN;

dphidy_true_ycut = dphidy_true(:, ix_cut);
dphidx_true_xcut = dphidx_true(iy_cut, :);
dphidy_fit_ycut = dphidy_fit(:, ix_cut);
dphidx_fit_xcut = dphidx_fit(iy_cut, :);


% Plot original, fitted, residual, coefficients, and coefficient error
figure;

tiledlayout(3, 6, 'TileSpacing', 'compact', 'Padding', 'compact');

nexttile(1, [1, 2])
imagesc(x, y, DPhaseNoisy);
hold on
xline(x0_cut, '-w', 'linewidth', 1), yline(y0_cut, '-w', 'linewidth', 1)
axis image;
colorbar;
title(sprintf('Original / noisy phase gradient (up to order %d)', nOrderOriginal));

nexttile(3, [1, 2])
imagesc(x, y, DPhaseFit);
hold on, xline(x0_cut, '-w', 'linewidth', 1), yline(y0_cut, '-w', 'linewidth', 1)
axis image off;
colorbar;
title('Zernike gradient fit');

nexttile(5, [1, 2])
imagesc(residual);
axis image off;
colorbar;

residVec = residual(fitMask);

rss = sum(residVec.^2, 'omitnan');
rmsResidual = sqrt(mean(residVec.^2, 'omitnan'));
pvResidual = max(residVec, [], 'omitnan') - min(residVec, [], 'omitnan');

title(sprintf('Residual: RSS = %.3g, RMS = %.3g', rss, rmsResidual));

nexttile(7,[1,2])
imagesc(x, y, phaseTrue);
hold on, xline(x0_cut, '-w', 'linewidth', 1), yline(y0_cut, '-w', 'linewidth', 1)
axis image off;
colorbar;
title('True phase');

nexttile(9,[1,2])
imagesc(x, y, phaseFit);
hold on, xline(x0_cut, '-w', 'linewidth', 1), yline(y0_cut, '-w', 'linewidth', 1)
axis image off;
colorbar;
title('Resulting phase');

nexttile(11,[1,2])
imagesc(x, y, phaseTrue - phaseFit);
hold on, xline(x0_cut, '-w', 'linewidth', 1), yline(y0_cut, '-w', 'linewidth', 1)
axis image off;
colorbar;
title('Phase residuals');

nexttile(13, [1, 3]);
bar([trueCoeffs, coeffs]);
grid on;
xlabel('Zernike mode');
ylabel('Coefficient');
title('True vs fitted coefficients');
legend({'True', 'Fit'}, 'Location', 'best');
xticks(1:numel(coeffs));
xticklabels(modeLabels);
xtickangle(90);

nexttile(16, [1, 3])
bar(coeffError);
grid on;
xlabel('Zernike mode');
ylabel('True - fit');
title('Coefficient error');
xticks(1:numel(coeffs));
xticklabels(modeLabels);
xtickangle(90);

sgtitle(sprintf('Zernike fit up to radial order %d', nOrderFit));
colormap(viridis)