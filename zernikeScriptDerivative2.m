% Demo script: generate fake Zernike phase map and fit using both x/y derivatives
% Requires:
%   fitZernikeDerivative2.m
%   zernikePolynomial.m
%   zernikePolynomialDerivative.m
%   viridis.m

clear; close all; clc;

% Parameters
Nx = 256;
Ny = 256;

nOrderOriginal = 3;
nOrderFit = nOrderOriginal;

cx = Nx/2;
cy = Ny/2;
radius = Nx/2 * 0.9;
center = [cy cx];

noiseStd = 0.1;

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

% True maps
phaseTrue = zeros(Ny, Nx);
DPhaseTrueX = zeros(Ny, Nx);
DPhaseTrueY = zeros(Ny, Nx);

% Choose true modes
trueModes = [];
for n = 0:nOrderOriginal
    for m = -n:2:n
        trueModes = [trueModes; n, m]; %#ok<AGROW>
    end
end

% Random coefficients
coeffScale = 0.5;
attenuationPower = 2;

radialOrders = trueModes(:, 1);
orderWeights = 1 ./ (radialOrders + 1).^attenuationPower;

randomCoeffs = coeffScale * orderWeights .* randn(size(trueModes, 1), 1);

randomCoeffs(1, :) = 0; % remove piston
trueTerms = [trueModes, randomCoeffs];

% Generate true phase and analytical derivatives
for k = 1:size(trueTerms, 1)
    n = trueTerms(k, 1);
    m = trueTerms(k, 2);
    c = trueTerms(k, 3);

    Z = zernikePolynomial(n, m, rho, theta, true);
    [Zx, Zy] = zernikePolynomialDerivative(n, m, rho, theta, true);

    phaseTrue = phaseTrue + c * Z;
    DPhaseTrueX = DPhaseTrueX + c * Zx;
    DPhaseTrueY = DPhaseTrueY + c * Zy;
end

% Mask outside aperture
phaseTrue(~pupilMask) = NaN;
DPhaseTrueX(~pupilMask) = NaN;
DPhaseTrueY(~pupilMask) = NaN;

% Add noise
DPhaseNoisyX = DPhaseTrueX;
DPhaseNoisyY = DPhaseTrueY;

DPhaseNoisyX(pupilMask) = DPhaseNoisyX(pupilMask) + noiseStd * randn(nnz(pupilMask), 1);
DPhaseNoisyY(pupilMask) = DPhaseNoisyY(pupilMask) + noiseStd * randn(nnz(pupilMask), 1);

% Fit coefficients using both x and y gradients
[coeffs, modes, DPhaseFitX, DPhaseFitY, residualX, residualY, fitMask] = ...
    fitZernikeDerivative2( ...
    DPhaseNoisyX, ...
    DPhaseNoisyY, ...
    nOrderFit, ...
    'Mask', pupilMask, ...
    'Center', center, ...
    'Radius', radius, ...
    'Normalize', true);

% Reconstruct fitted phase from fitted coefficients
phaseFit = zeros(Ny, Nx);

for k = 1:size(modes, 1)
    n = modes(k, 1);
    m = modes(k, 2);
    c = coeffs(k);

    Z = zernikePolynomial(n, m, rho, theta, true);
    phaseFit = phaseFit + c * Z;
end

phaseFit(~pupilMask) = NaN;

% Print coefficient comparison
fprintf('\nZernike coefficient comparison:\n');
fprintf('----------------------------------------------------\n');
fprintf('%5s %5s %14s %14s %14s\n', ...
    'n', 'm', 'true coeff', 'fit coeff', 'error');
fprintf('----------------------------------------------------\n');

trueCoeffs = zeros(size(coeffs));

for j = 1:size(modes, 1)
    n = modes(j, 1);
    m = modes(j, 2);

    idx = trueTerms(:, 1) == n & trueTerms(:, 2) == m;

    if any(idx)
        trueCoeffs(j) = trueTerms(idx, 3);
    end

    fitCoeff = coeffs(j);
    coeffErr = fitCoeff - trueCoeffs(j);

    fprintf('%5d %5d %14.6f %14.6f %14.6f\n', ...
        n, m, trueCoeffs(j), fitCoeff, coeffErr);
end

fprintf('----------------------------------------------------\n');

coeffError = trueCoeffs - coeffs;

modeLabels = strings(size(modes, 1), 1);
for j = 1:size(modes, 1)
    modeLabels(j) = sprintf('(%d,%+d)', modes(j, 1), modes(j, 2));
end

% Cut positions
x0_cut = -0.;
y0_cut = -0.;

[~, ix_cut] = min(abs(x(:) - x0_cut));
[~, iy_cut] = min(abs(y(:) - y0_cut));

% Cuts through phase
phase_true_xcut = phaseTrue(iy_cut, :);
phase_fit_xcut  = phaseFit(iy_cut, :);

phase_true_ycut = phaseTrue(:, ix_cut);
phase_fit_ycut  = phaseFit(:, ix_cut);

% Cuts through derivatives
dphidx_true_xcut = DPhaseTrueX(iy_cut, :);
dphidx_fit_xcut  = DPhaseFitX(iy_cut, :);

dphidy_true_ycut = DPhaseTrueY(:, ix_cut);
dphidy_fit_ycut  = DPhaseFitY(:, ix_cut);

% Plot
figure;

tiledlayout(4, 6, 'TileSpacing', 'compact', 'Padding', 'compact');

nexttile(1, [1, 2])
imagesc(x, y, DPhaseNoisyX);
hold on;
xline(x0_cut, '-w', 'LineWidth', 1);
yline(y0_cut, '-w', 'LineWidth', 1);
axis image;
colorbar;
title('Noisy d\phi/dx');

nexttile(3, [1, 2])
imagesc(x, y, DPhaseFitX);
hold on;
xline(x0_cut, '-w', 'LineWidth', 1);
yline(y0_cut, '-w', 'LineWidth', 1);
axis image;
colorbar;
title('Fit d\phi/dx');

nexttile(5, [1, 2])
imagesc(x, y, residualX);
axis image;
colorbar;
title('Residual d\phi/dx');

nexttile(7, [1, 2])
imagesc(x, y, DPhaseNoisyY);
hold on;
xline(x0_cut, '-w', 'LineWidth', 1);
yline(y0_cut, '-w', 'LineWidth', 1);
axis image;
colorbar;
title('Noisy d\phi/dy');

nexttile(9, [1, 2])
imagesc(x, y, DPhaseFitY);
hold on;
xline(x0_cut, '-w', 'LineWidth', 1);
yline(y0_cut, '-w', 'LineWidth', 1);
axis image;
colorbar;
title('Fit d\phi/dy');

nexttile(11, [1, 2])
imagesc(x, y, residualY);
axis image;
colorbar;
title('Residual d\phi/dy');

nexttile(13, [1, 2])
imagesc(x, y, phaseTrue);
hold on;
xline(x0_cut, '-w', 'LineWidth', 1);
yline(y0_cut, '-w', 'LineWidth', 1);
axis image;
colorbar;
title('True phase');

nexttile(15, [1, 2])
imagesc(x, y, phaseFit);
hold on;
xline(x0_cut, '-w', 'LineWidth', 1);
yline(y0_cut, '-w', 'LineWidth', 1);
axis image;
colorbar;
title('Recovered phase');

nexttile(17, [1, 2])
phaseResidual = phaseTrue - phaseFit;
% phaseResidual = phaseResidual - mean(phaseResidual(:), 'omitnan');
imagesc(x, y, phaseResidual);
axis image;
colorbar;
title('Phase residual');

nexttile(19, [1, 3])
bar([trueCoeffs, coeffs]);
grid on;
xlabel('Zernike mode');
ylabel('Coefficient');
title('True vs fitted coefficients');
legend({'True', 'Fit'}, 'Location', 'best');
xticks(1:numel(coeffs));
xticklabels(modeLabels);
xtickangle(90);

nexttile(22, [1, 3])
bar(coeffError);
grid on;
xlabel('Zernike mode');
ylabel('True - fit');
title('Coefficient error');
xticks(1:numel(coeffs));
xticklabels(modeLabels);
xtickangle(90);

sgtitle(sprintf('2D derivative Zernike fit up to radial order %d', nOrderFit));
colormap(viridis);

% % Optional second figure: cuts
% figure;
% tiledlayout(2, 2, 'TileSpacing', 'compact', 'Padding', 'compact');
% 
% nexttile;
% plot(x, phase_true_xcut, 'k-', x, phase_fit_xcut, 'r--', 'LineWidth', 1.5);
% grid on;
% xlabel('x');
% ylabel('\phi');
% title(sprintf('Phase cut at y = %.3f', y(iy_cut)));
% legend('True', 'Fit');
% 
% nexttile;
% plot(y, phase_true_ycut, 'k-', y, phase_fit_ycut, 'r--', 'LineWidth', 1.5);
% grid on;
% xlabel('y');
% ylabel('\phi');
% title(sprintf('Phase cut at x = %.3f', x(ix_cut)));
% legend('True', 'Fit');
% 
% nexttile;
% plot(x, dphidx_true_xcut, 'k-', x, dphidx_fit_xcut, 'r--', 'LineWidth', 1.5);
% grid on;
% xlabel('x');
% ylabel('\partial\phi/\partialx');
% title(sprintf('d\\phi/dx cut at y = %.3f', y(iy_cut)));
% legend('True', 'Fit');
% 
% nexttile;
% plot(y, dphidy_true_ycut, 'k-', y, dphidy_fit_ycut, 'r--', 'LineWidth', 1.5);
% grid on;
% xlabel('y');
% ylabel('\partial\phi/\partialy');
% title(sprintf('d\\phi/dy cut at x = %.3f', x(ix_cut)));
% legend('True', 'Fit');

sgtitle('Phase and derivative cuts');