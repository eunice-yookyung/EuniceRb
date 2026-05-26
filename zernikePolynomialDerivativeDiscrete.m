function [Zx, Zy] = zernikePolynomialDerivativeDiscrete(n, m, rho, theta, normalize, dx, dy)

    if nargin < 7 || isempty(dy)
        dy = dx;
    end

    x = rho .* cos(theta);
    y = rho .* sin(theta);

    % x derivative: symmetric points
    xp = x + dx/2;
    xm = x - dx/2;

    rho_xp = sqrt(xp.^2 + y.^2);
    theta_xp = atan2(y, xp);

    rho_xm = sqrt(xm.^2 + y.^2);
    theta_xm = atan2(y, xm);

    Zxp = zernikePolynomial(n, m, rho_xp, theta_xp, normalize);
    Zxm = zernikePolynomial(n, m, rho_xm, theta_xm, normalize);

    Zx = (Zxp - Zxm) / dx;

    % y derivative: symmetric points
    yp = y + dy/2;
    ym = y - dy/2;

    rho_yp = sqrt(x.^2 + yp.^2);
    theta_yp = atan2(yp, x);

    rho_ym = sqrt(x.^2 + ym.^2);
    theta_ym = atan2(ym, x);

    Zyp = zernikePolynomial(n, m, rho_yp, theta_yp, normalize);
    Zym = zernikePolynomial(n, m, rho_ym, theta_ym, normalize);

    Zy = (Zyp - Zym) / dy;

    % Points are valid only if both symmetric samples stay inside unit disk
    Zx(rho_xp > 1 | rho_xm > 1) = NaN;
    Zy(rho_yp > 1 | rho_ym > 1) = NaN;
end