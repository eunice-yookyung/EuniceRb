% rf plot, for experiment

is_pdf = 0; % 0 = jpg, 1 = pdf
folder = 'damop_plots';
rfplot_damop();
fig = gcf;
fig2pdf(fig, sprintf('%s/fig_rf_process', folder), is_pdf)
%%
n = 2;
[tileObj, data, files] = plot_csv_data_damop(2);
rfplot_damop(tileObj)
nexttile(5)
ylabel('Observed normalized scattering')
ax=gca;
ax.Children(4).XData(end)=nan;
ax.Children(2).XData(end)=nan;
% ax.YLim = [0,1];
nexttile(6)
ylabel('Elastic scattering per atom')
ax=gca;
ax.Children(4).XData(end)=nan;
ax.Children(2).XData(end)=nan;
%%
h = figure;
set(h, 'Units', 'inches', 'Position', [2, 2, 3.5, 3.5])
black = [0,0,0];
darkRed= red * .9;
col = [black; red];
for i = [5,3]
    errorbar(data{i}.hole_frac(1:end-1), data{i}.(2)(1:end-1), data{i}.(3)(1:end-1), ...
        '.', 'capsize', 0, 'linewidth', 1, 'markerSize', 10)
    hold on
end
for i = 1:2
    plot(data{2}.hole_frac, data{2}.(i+1), 'LineWidth', 1)
end
set(gca, 'ColorOrder', [black;darkRed;black;darkRed])
ylim([-.1,2.1]), yticks(0:.5:2)
text(.4,.2,'Singlon','Color','k'), text(.4,1.5,'Doublon','Color',darkRed), xlabel('Hole fraction'), ylabel('Normalized elastic scattering')
fig2pdf(h, sprintf('%s/dicke', folder), is_pdf)

%% -------------------
% harmonic oscillator
is_pdf = 0; % 0 = jpg, 1 = pdf

colors = load("C:\Users\eunic\EuniceRb\DAMOP2026\Doublons\my_colors.mat").colors_blue_red;
colors = (colors(1:6, :));

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