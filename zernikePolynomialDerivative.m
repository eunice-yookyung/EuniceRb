function [Zx, Zy] = zernikePolynomialDerivative(n, m, rho, theta, normalize)
%ZERNIKEPOLYNOMIALDERIVATIVE Cartesian derivatives of real Zernike polynomial.
%
%   [Zx, Zy] = zernikePolynomialDerivative(n, m, rho, theta, normalize)
%
%   Returns derivatives with respect to normalized Cartesian coordinates:
%       x = rho*cos(theta)
%       y = rho*sin(theta)

    absM = abs(m);

    R  = zernikeRadial(n, absM, rho);
    dR = zernikeRadialDerivative(n, absM, rho);

    if m > 0
        angular = cos(absM * theta);
        dAngular_dtheta = -absM * sin(absM * theta);

        normFactor = sqrt(2 * (n + 1));

    elseif m < 0
        angular = sin(absM * theta);
        dAngular_dtheta = absM * cos(absM * theta);

        normFactor = sqrt(2 * (n + 1));

    else
        angular = ones(size(theta));
        dAngular_dtheta = zeros(size(theta));

        normFactor = sqrt(n + 1);
    end

    if ~normalize
        normFactor = 1;
    end

    dZdrho   = normFactor * dR .* angular;
    dZdtheta = normFactor * R  .* dAngular_dtheta;

    cosTheta = cos(theta);
    sinTheta = sin(theta);

    invRho = zeros(size(rho));
    invRho(rho ~= 0) = 1 ./ rho(rho ~= 0);

    Zx = cosTheta .* dZdrho - sinTheta .* invRho .* dZdtheta;
    Zy = sinTheta .* dZdrho + cosTheta .* invRho .* dZdtheta;

    % Handle rho = 0 safely using finite values
    Zx(rho == 0) = 0;
    Zy(rho == 0) = 0;
end


function R = zernikeRadial(n, m, rho)
    R = zeros(size(rho));

    if mod(n - m, 2) ~= 0
        return;
    end

    for k = 0:((n - m) / 2)
        coeff = (-1)^k ...
            * factorial(n - k) ...
            / (factorial(k) ...
            * factorial((n + m) / 2 - k) ...
            * factorial((n - m) / 2 - k));

        R = R + coeff * rho.^(n - 2 * k);
    end
end


function dR = zernikeRadialDerivative(n, m, rho)
    dR = zeros(size(rho));

    if mod(n - m, 2) ~= 0
        return;
    end

    for k = 0:((n - m) / 2)
        powerVal = n - 2 * k;

        if powerVal == 0
            continue;
        end

        coeff = (-1)^k ...
            * factorial(n - k) ...
            / (factorial(k) ...
            * factorial((n + m) / 2 - k) ...
            * factorial((n - m) / 2 - k));

        dR = dR + coeff * powerVal * rho.^(powerVal - 1);
    end
end