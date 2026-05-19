function [coeffs, modes, phaseFit, residual, pupilMask] = fitZernike(phase, nOrder, varargin)
%FITZERNIKE Fit a 2D phase map with real-valued Zernike polynomials.
%
%   [coeffs, modes, phaseFit, residual, pupilMask] = fitZernike(phase, nOrder)
%
%   Fits phase using all Zernike modes up to radial order nOrder.
%
%   Optional name-value arguments:
%       'Mask'      : logical valid-pixel mask, same size as phase
%       'Center'    : [cy cx] pupil center in pixel coordinates
%       'Radius'    : pupil radius in pixels
%       'Normalize' : true/false, use orthonormal normalization
%
%   Outputs:
%       coeffs    : fitted coefficients
%       modes     : N_modes x 2 array of [n m] mode indices
%       phaseFit  : reconstructed phase map
%       residual  : phase - phaseFit
%       pupilMask : final mask used for fitting
%
%   Convention:
%       m > 0 : cosine term
%       m < 0 : sine term
%       m = 0 : radial-only term

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

    phase = double(phase);
    [Ny, Nx] = size(phase);

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

    pupilMask = pupilMask & isfinite(phase);

    modes = zernikeModeList(nOrder);

    b = phase(pupilMask);
    nPixels = numel(b);
    nModes = size(modes, 1);

    A = zeros(nPixels, nModes);

    for j = 1:nModes
        n = modes(j, 1);
        m = modes(j, 2);

        Z = zernikePolynomial(n, m, rho, theta, normalize);
        A(:, j) = Z(pupilMask);
    end

    coeffs = A \ b;

    fitValues = A * coeffs;

    phaseFit = nan(size(phase));
    phaseFit(pupilMask) = fitValues;

    residual = nan(size(phase));
    residual(pupilMask) = phase(pupilMask) - fitValues;
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