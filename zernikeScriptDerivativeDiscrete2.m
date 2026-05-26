% testZernikeDiscreteDerivative.m
%
% Compare:
%   1) true Zernike phase map
%   2) analytical derivatives
%   3) discrete derivatives
%
% The key check:
%   integrating the derivative behavior should correspond to the SAME
%   underlying phase polynomial, not another derivative mode.

clear; close all; clc;

% Grid

Nx = 256;
Ny = 256;

cx = Nx/2;
cy = Ny/2;

radius = Nx/2 * 0.9;

x_ = 1:Nx;
y_ = 1:Ny;

[X_, Y_] = meshgrid(x_, y_);

x = (x_ - cx) / radius;
y = (y_ - cy) / radius;

X = (X_ - cx) / radius;
Y = (Y_ - cy) / radius;

rho   = sqrt(X.^2 + Y.^2);
theta = atan2(Y, X);

mask = rho <= 1;

dx = mean(diff(x)) * 100;
dy = mean(diff(y)) * 100;

% Build phase map from known coefficients

% [n m coeff]
terms = [
    2   0   0.8   % defocus
    2   2   0.2   % astigmatism
    3   1   0.15  % coma
];

Ztrue  = zeros(size(rho));
ZxAnalytical = zeros(size(rho));
ZyAnalytical = zeros(size(rho));

ZxDisc = zeros(size(rho));
ZyDisc = zeros(size(rho));

for k = 1:size(terms,1)

    n = terms(k,1);
    m = terms(k,2);
    c = terms(k,3);

    % actual phase polynomial
    Z = zernikePolynomial(n, m, rho, theta, true);

    % analytical derivative
    [Zx, Zy] = zernikePolynomialDerivative( ...
        n, m, rho, theta, true);

    % discrete derivative
    [Zx_d, Zy_d] = zernikePolynomialDerivativeDiscrete( ...
        n, m, rho, theta, true, dx, dy);

    % accumulate SAME coefficients
    Ztrue  = Ztrue  + c * Z;

    ZxAnalytical = ZxAnalytical + c * Zx;
    ZyAnalytical = ZyAnalytical + c * Zy;

    ZxDisc = ZxDisc + c * Zx_d;
    ZyDisc = ZyDisc + c * Zy_d;
end

% Mask outside pupil

Ztrue(~mask)  = NaN;

ZxAnalytical(~mask) = NaN;
ZyAnalytical(~mask) = NaN;

ZxDisc(~mask) = NaN;
ZyDisc(~mask) = NaN;

% Errors

errX = ZxDisc - ZxAnalytical;
errY = ZyDisc - ZyAnalytical;

rmsX = rms(errX(mask), 'omitnan');
rmsY = rms(errY(mask), 'omitnan');

fprintf('RMS error X derivative: %.6e\n', rmsX);
fprintf('RMS error Y derivative: %.6e\n', rmsY);

% Cuts

[~, iy0] = min(abs(y - 0));
[~, ix0] = min(abs(x - 0));

phase_xcut = Ztrue(iy0,:);

zxAnal_xcut = ZxAnalytical(iy0,:);
zxDisc_xcut = ZxDisc(iy0,:);

zyAnal_ycut = ZyAnalytical(:,ix0);
zyDisc_ycut = ZyDisc(:,ix0);

% Plot

figure;

tiledlayout(4,4,'TileSpacing','compact');

% -------------------------------------------------
nexttile;
imagesc(x, y, Ztrue);
axis image;
colorbar;
title('Phase Z');

% -------------------------------------------------
nexttile;
imagesc(x, y, ZxAnalytical);
axis image;
colorbar;
title('Analytical dZ/dx');

nexttile;
imagesc(x, y, ZxDisc);
axis image;
colorbar;
title('Discrete dZ/dx');

nexttile;
imagesc(x, y, errX);
axis image;
colorbar;
title('dZ/dx error');

% -------------------------------------------------
nexttile;
imagesc(x, y, Ztrue);
axis image;
colorbar;
title('Phase Z');

nexttile;
imagesc(x, y, ZyAnalytical);
axis image;
colorbar;
title('Analytical dZ/dy');

nexttile;
imagesc(x, y, ZyDisc);
axis image;
colorbar;
title('Discrete dZ/dy');

nexttile;
imagesc(x, y, errY);
axis image;
colorbar;
title('dZ/dy error');

% -------------------------------------------------
nexttile([1 2]);
plot(x, zxAnal_xcut, 'k-', 'LineWidth', 2);
hold on;
plot(x, zxDisc_xcut, 'r--', 'LineWidth', 1.5);
grid on;

xlabel('x');
ylabel('dZ/dx');

title('X derivative cut');
legend('Analytical','Discrete');

% -------------------------------------------------
nexttile([1 2]);
plot(y, zyAnal_ycut, 'k-', 'LineWidth', 2);
hold on;
plot(y, zyDisc_ycut, 'r--', 'LineWidth', 1.5);
grid on;

xlabel('y');
ylabel('dZ/dy');

title('Y derivative cut');
legend('Analytical','Discrete');

sgtitle('Discrete vs analytical Zernike derivatives');

% -------------------------------------------------
nexttile([1 2]);
plot(x, zxAnal_xcut - zxDisc_xcut, 'k-', 'LineWidth', 2);
grid on;

xlabel('x');
ylabel('dZ/dx residuals');

title('X Residuals');
legend('Analytical','Discrete');

% -------------------------------------------------
nexttile([1 2]);
plot(y, zyAnal_ycut - zyDisc_ycut, 'k-', 'LineWidth', 2);
hold on;
grid on;

xlabel('y');
ylabel('dZ/dy');

title('Y Residuals ');
legend('Analytical','Discrete');

sgtitle('Discrete vs analytical Zernike derivatives');