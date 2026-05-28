% busch wavefunctions

E = -1:.01:7/2; a = E2a(E);
figure, plot(-1./a, E, '-k')

%%
fig_busch = figure('Units','inches','Position',[2,2,3,2.5]);
r = 0.01:.05:4;
aosc = 100; 

colors = load("C:\Users\eunic\EuniceRb\DAMOP2026\Doublons\my_colors.mat").colors_blue_red;
colors(1,:) = [0,0,0];
colors(2,:) = colors(end-2,:);

for anm = [0.01, -40, 20, 70]
    a = anm / aosc; E = a2E(a); v = 0.5 * (E - 3/2); 
    psi = exp(-r.^2 / 2) * gamma(-v) .* kummerU(-v, 3/2, r.^2);
    y = 4 * pi * r.^2 .* abs(psi).^2;
    ynorm = trapz(r, y);
    y = y / ynorm;
    plot(r * aosc / sqrt(2), y, 'linewidth', 2, 'DisplayName', sprintf('a_s = %d nm', round(anm))), hold on
end
set(gca,'ColorOrder',colors), legend('FontSize', 8)
xlabel('Relative distance (nm)'), ylabel('r^2|\psi|^2 (arb. u.)'), xlim([0, 250])

is_pdf = 0; % 0 = jpg, 1 = pdf
folder = 'damop_plots';
fig2pdf(fig_busch, sprintf('%s/busch_wavefunctions', folder), is_pdf)

function a = E2a(E)
a = (sqrt(2) * gamma(-E/2 + 3/4) ./ gamma(-E/2 + 1/4)).^(-1);
end

function E0 = a2E(a0)
E = 0.51:.01:2.49;
a = (sqrt(2) * gamma(-E/2 + 3/4) ./ gamma(-E/2 + 1/4)).^(-1);
E0 = interp1(a, E, a0);
end