% get the structure factor

clear

fig = figure('Units','inches','Position',[2,2,10,5]);
% Structure factor for 3D object
% pHole_list = [0, .5, .9];
pHole_list = [0, .5, .9];
tiledlayout(1,length(pHole_list),'TileSpacing','tight','Padding','tight')

for pHole = pHole_list  % probability that a site inside radius R is empty

    t0 = tic;

    theta = linspace(0, pi, 200);
    phi   = linspace(0, pi/2, 100);

    R = 15;
    x = -R:R;
    y = x;
    z = x;

    [X, Y, Z] = meshgrid(x, y, z);
    insideSphere = X.^2 + Y.^2 + Z.^2 <= R^2;
    C = insideSphere & (rand(size(X)) > pHole);

    % Keep only occupied/object coordinates
    xx = X(C);
    yy = Y(C);
    zz = Z(C);

    alat = 532;
    lam  = 671;
    q = 2*pi/lam * alat;

    % Angle grids
    [Theta, Phi] = ndgrid(theta, phi);

    qx = q .* cos(Phi) .* sin(Theta);
    qy = q .* sin(Phi) .* sin(Theta);
    qz = q .* (1 - cos(Theta));

    % Flatten q-space grid
    qxv = qx(:);
    qyv = qy(:);
    qzv = qz(:);

    % Vectorized structure factor amplitude
    phase = qxv * xx(:).' + qyv * yy(:).' + qzv * zz(:).';

    A = sum(exp(1i * phase), 2);

    % Reshape back to theta x phi
    A = reshape(A, length(theta), length(phi));

    S = abs(A).^2 / sum(C(:));

    nexttile
    myimshow(phi/pi, theta/pi, flipud(S)), hold on
    yt = yticks; yticklabels(1-yt)
    th = 0:.01:1; xc = .2 * cos(th * 2 * pi) + pi/4; yc = .2 * sin(th * 2 * pi) + pi/2;
    plot(xc/pi, yc/pi, '-w', 'linewidth', 1), clim([0,2])
    set(gca,'FontSize', 12)
    % title(sprintf('S(\\phi, \\theta). N = %d atoms (R = %d sites)', sum(C(:)), R))
    title(sprintf('Hole fraction = %0.2f', pHole))
    sgtitle('Structure factor','FontSize',20)
    xlabel('\phi (\pi)'), ylabel('\theta (\pi)'), colormap(inferno)
    if pHole == 0
        text(.25, .6, 'Collection angle', 'Color', 'w', 'FontSize', 12, 'HorizontalAlignment', 'center')
    end

    t1 = toc(t0);
    fprintf('Elapsed time = %0.2f seconds.\n', t1)

end

%%

is_pdf = 0; % 0 = jpg, 1 = pdf
folder = 'damop_plots';
fig2pdf(fig, sprintf('%s/Sq_holes', folder), is_pdf)
