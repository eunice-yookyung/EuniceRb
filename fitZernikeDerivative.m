function [coeffs, modes, derivFit, residual, pupilMask] = fitZernikeDerivative(deriv, nOrder, varargin)
%FITZERNIKE DERIVATIVE Fit a 2D derivative map with Zernike derivatives.
%
%   [coeffs, modes, derivFit, residual, pupilMask] = ...
%       fitZernikeDerivative(deriv, nOrder)
%
%   Defaults to fitting dZ/dx.
%
%   Optional name-value arguments:
%       'Gradient'  : 'x' or 'y', default 'x'
%       'Mask'      : logical valid-pixel mask, same size as deriv
%       'Center'    : [cy cx] pupil center in pixel coordinates
%       'Radius'    : pupil radius in pixels
%       'Normalize' : true/false, use orthonormal normalization

    p = inputParser;
    p.addParameter('Gradient', 'x', @(s) ischar(s) || isstring(s));
    p.addParameter('Mask', [], @(x) isempty(x) || islogical(x) || isnumeric(x));
    p.addParameter('Center', [], @(x) isempty(x) || numel(x) == 2);
    p.addParameter('Radius', [], @(x) isempty(x) || isscalar(x));
    p.addParameter('Normalize', true, @(x) islogical(x) || isnumeric(x));
    p.parse(varargin{:});

    gradientDir = lower(string(p.Results.Gradient));
    userMask    = p.Results.Mask;
    center      = p.Results.Center;
    radius      = p.Results.Radius;
    normalize   = logical(p.Results.Normalize);

    deriv = double(deriv);
    [Ny, Nx] = size(deriv);

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

    pupilMask = pupilMask & isfinite(deriv);

    modes = zernikeModeList(nOrder);

    b = deriv(pupilMask);
    nPixels = numel(b);
    nModes = size(modes, 1);

    A = zeros(nPixels, nModes);

    for j = 1:nModes
        n = modes(j, 1);
        m = modes(j, 2);

        [Zx, Zy] = zernikePolynomialDerivative(n, m, rho, theta, normalize);

        if gradientDir == "x"
            Zderiv = Zx;
        elseif gradientDir == "y"
            Zderiv = Zy;
        else
            error("Gradient must be 'x' or 'y'.");
        end

        A(:, j) = Zderiv(pupilMask);
    end

    % Remove unobservable zero columns, e.g. piston derivative
    activeCols = vecnorm(A) > 1e-12;

    coeffs = zeros(nModes, 1);
    coeffs(activeCols) = A(:, activeCols) \ b;

    fitValues = A * coeffs;

    derivFit = nan(size(deriv));
    derivFit(pupilMask) = fitValues;

    residual = nan(size(deriv));
    residual(pupilMask) = deriv(pupilMask) - fitValues;
end


function modes = zernikeModeList(nOrder)
%ZERNIKE_MODE_LIST Return [n m] pairs up to radial order nOrder.

    modes = [];

    for n = 0:nOrder
        for m = -n:2:n
            modes = [modes; n, m]; %#ok<AGROW>
        end
    end
end