function Z = zernikePolynomial(n, m, rho, theta, normalize)
%ZERNIKEPOLYNOMIAL Real-valued Zernike polynomial.
%
%   m > 0 : R_n^m(rho) * cos(m theta)
%   m < 0 : R_n^|m|(rho) * sin(|m| theta)
%   m = 0 : R_n^0(rho)
%
% Common aberration names for low-order real Zernike modes:
%
%   n   m      Common name
%   -----------------------------------------------
%   0   0      Piston
%
%   1  -1      Y tilt
%   1   1      X tilt
%
%   2  -2      Oblique astigmatism / 45-degree astigmatism
%   2   0      Defocus
%   2   2      Vertical astigmatism / 0-degree astigmatism
%
%   3  -3      Oblique trefoil
%   3  -1      Vertical coma / Y coma
%   3   1      Horizontal coma / X coma
%   3   3      Vertical trefoil
%
%   4  -4      Oblique quadrafoil
%   4  -2      Oblique secondary astigmatism
%   4   0      Primary spherical aberration
%   4   2      Vertical secondary astigmatism
%   4   4      Vertical quadrafoil
%
% Notes:
%   m > 0 modes use cos(m*theta)
%   m < 0 modes use sin(abs(m)*theta)
%   Exact sign/orientation names may vary by convention.

    R = zernikeRadial(n, abs(m), rho);

    if m > 0
        Z = R .* cos(m * theta);

        if normalize
            Z = Z * sqrt(2 * (n + 1));
        end

    elseif m < 0
        Z = R .* sin(abs(m) * theta);

        if normalize
            Z = Z * sqrt(2 * (n + 1));
        end

    else
        Z = R;

        if normalize
            Z = Z * sqrt(n + 1);
        end
    end
end


function R = zernikeRadial(n, m, rho)
%ZERNIKE_RADIAL Radial Zernike polynomial R_n^m(rho).

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