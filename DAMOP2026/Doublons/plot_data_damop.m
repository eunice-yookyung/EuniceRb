% rf plot, for experiment

is_pdf = 0; % 0 = jpg, 1 = pdf
folder = 'damop_plots';
rfplot_damop();
fig = gcf;
fig2pdf(fig, sprintf('%s/fig_rf_process', folder), is_pdf)

%% -------------------
% harmonic oscillator
is_pdf = 0; % 0 = jpg, 1 = pdf

colors = load("C:\Users\eunic\EuniceRb\DAMOP2026\Doublons\my_colors.mat").colors_teal_brown;
colors = flipud(colors(5:end, :));

x = -5:.01:5;
f = @(x_, n_) 1/sqrt(2.^n_ * factorial(n_)) * pi^(-1/4) .* exp(-x_.^2 / 2) .* hermiteH(n_, x_);

fig = figure('Units','inches','Position',[1,1,3.5,3.5],'Color','w');
plot(x, x.^2*.1, 'linewidth', 2, 'Color', 'k'), hold on
plot(x, f(x, 0)/2+.5, 'linewidth', 2), xlabel('x / a_{osc}')
plot(x, f(x, 1)/2 + 1.5, 'linewidth', 2), xlabel('x / a_{osc}'), ylabel(['E / ' mychar('hbar') mychar('omega')])
plot(x, [0*x+0.5; 0*x+1.5], ':', 'linewidth', 1, 'Color', 'k')
ylim([-.1,2.3])
set(gca, 'ColorOrder', colors, 'Linewidth', 1, 'FontSize', 12), yticks([]), xticks([])
fig2pdf(fig, sprintf('%s/harmonic_oscillator_wavefunctions', folder), is_pdf)

%% ---------------
is_pdf = 0; % 0 = jpg, 1 = pdf
blue = [41, 171, 256] / 256;
red = [1, 0.5, 0.5];          % light red
folder = 'damop_plots';
plot_csv_data_damop(5, 'none');
fig = gcf;
fig2pdf(fig, sprintf('%s/pulse_duration', folder), is_pdf)
nexttile(1); text(5,.05,'Singlons', 'Color', 'k'), text(5,.2,'Doublons', 'Color', red)
fig2pdf(fig, sprintf('%s/pulse_duration', folder), is_pdf)

%% ---------------
is_pdf = 0; % 0 = jpg, 1 = pdf
blue = [41, 171, 256] / 256;
red = [1, 0.5, 0.5];          % light red
dark = [1, 0.5, 0.5] / 2;
folder = 'damop_plots';
plot_csv_data_damop(6, 'none');
fig = gcf;
fig2pdf(fig, sprintf('%s/pulse_duration', folder), is_pdf)
nexttile(2); text(-40,.25,'Singlons', 'Color', 'k'), text(-40,1.0,'Theory', 'Color', red), text(10,1.0,'Doublons', 'Color', dark)
nexttile(1); legend(['0 ' mychar('mu') 's'],'','','',['3.3 ' mychar('mu') 's'])
fig2pdf(fig, sprintf('%s/scattering_length', folder), is_pdf)