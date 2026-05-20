function [coeffs, modes, ZxFit, ZyFit, residualX, residualY, pupilMask] = ...
    fitZernikeDerivative2(ZxData, ZyData, nOrder, varargin)
%FITZERNIKEDERIVATIVE2 Fit Zernike coefficients using both x and y gradients.
%
%   [coeffs, modes, ZxFit, ZyFit, residualX, residualY, pupilMask] = ...
%       fitZernikeDerivative2(ZxData, ZyData, nOrder)
%
%   Fits:
%       ZxData ≈ sum_j coeffs(j) * dZ_j/dx
%       ZyData ≈ sum_j coeffs(j) * dZ_j/dy

    p = inputParser;
    p.addParameter('Mask', [], @(x) isempty(x) || islogical(x) || isnumeric(x));
    p.addParameter('Center', [], @(x) isempty(x) || numel(x) == 2);
    p.addParameter('Radius', [], @(x) isempty(x) || isscalar(x));
    p.addParameter('Normalize', true, @(x) islogical(x) || isnumeric(x));
    p.parse(varargin{:});

    userMask  = p.Results.Mask;
    center    = p.Results.Center;
    radius    = p.Results.Radius;
    normalize = logical(p.Results.Normalize);

    ZxData = double(ZxData);
    ZyData = double(ZyData);

    if ~isequal(size(ZxData), size(ZyData))
        error('ZxData and ZyData must have the same size.');
    end

    [Ny, Nx] = size(ZxData);

    if isempty(center)
        cy = (Ny + 1) / 2;
        cx = (Nx + 1) / 2;
    else
        cy = center(1);
        cx = center(2);
    end

    if isempty(radius)
        radius = min(Nx, Ny) / 2;
    end

    [xx, yy] = meshgrid(1:Nx, 1:Ny);

    x = (xx - cx) / radius;
    y = (yy - cy) / radius;

    rho = sqrt(x.^2 + y.^2);
    theta = atan2(y, x);

    pupilMask = rho <= 1;

    if ~isempty(userMask)
        pupilMask = pupilMask & logical(userMask);
    end

    pupilMask = pupilMask & isfinite(ZxData) & isfinite(ZyData);

    modes = zernikeModeList(nOrder);

    nPixels = nnz(pupilMask);
    nModes = size(modes, 1);

    Ax = zeros(nPixels, nModes);
    Ay = zeros(nPixels, nModes);

    for j = 1:nModes
        n = modes(j, 1);
        m = modes(j, 2);

        [ZxBasis, ZyBasis] = zernikePolynomialDerivative(n, m, rho, theta, normalize);

        Ax(:, j) = ZxBasis(pupilMask);
        Ay(:, j) = ZyBasis(pupilMask);
    end

    A = [Ax; Ay];
    b = [ZxData(pupilMask); ZyData(pupilMask)];

    % Remove unobservable columns, especially piston
    activeCols = vecnorm(A) > 1e-12;

    coeffs = zeros(nModes, 1);
    coeffs(activeCols) = A(:, activeCols) \ b;

    ZxFitVals = Ax * coeffs;
    ZyFitVals = Ay * coeffs;

    ZxFit = nan(size(ZxData));
    ZyFit = nan(size(ZyData));

    ZxFit(pupilMask) = ZxFitVals;
    ZyFit(pupilMask) = ZyFitVals;

    residualX = nan(size(ZxData));
    residualY = nan(size(ZyData));

    residualX(pupilMask) = ZxData(pupilMask) - ZxFitVals;
    residualY(pupilMask) = ZyData(pupilMask) - ZyFitVals;
end


function modes = zernikeModeList(nOrder)
%ZERNIKEMODELIST Return [n m] pairs up to radial order nOrder.

    modes = [];

    for n = 0:nOrder
        for m = -n:2:n
            modes = [modes; n, m]; %#ok<AGROW>
        end
    end
end