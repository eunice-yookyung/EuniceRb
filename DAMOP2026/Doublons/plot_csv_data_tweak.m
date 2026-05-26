% tweak figs

%% Dicke superradiance plot
n = 2;
[tileObj, data, files] = plot_csv_data(n, 'no save');
rfplot(tileObj)
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

%% Tweak fig 4 to include text and make singlons line dashed

plot_csv_data(4, '');

labelFontsize = 8;

n = 4;
figure(n);
h=gcf;
h.Position=[1,1,3.5,2.5];

ax = nexttile(1);
ax.Children(2).LineStyle = '--';
ax.Children(2).LineWidth = 1;
title('Time-of-flight data')
text(50,.2,{'Resonantly', 'ejected atoms'},'FontSize',labelFontsize)
plot([90,97],[.16,.11],'-k')
xlabel(['Time-of-flight distance (' mychar('mu') 'm)'])
ylabel('Radial density')
ylabel('Integrated atomic density')
legend(['   0 ' mychar('mu') 's'],'','','',['3.3 ' mychar('mu') 's'], ...
    'Position',[.25,.7,.225,.12])

nexttile(2);
text(60,.5,{'Resonant', 'region'},'FontSize',labelFontsize)
plot([50,70],[.2,.4],'-k')

fig2pdf(figure(n),['figure' num2str(n)])

%% Tweak fig 5 to include spline to guide the eye

plot_csv_data(5, 'no save');

n = 5;
figure(n);
h=gcf;
h.Position=[1,1,3.5,4];

for i = 1:2
    ax = nexttile(i);
    lines = ax.Children;
    
    for j = [3,2]
        xx = lines(j).XData';
        yy = lines(j).YData';
        % lines(j).LineStyle = ':';
        % fs = fit(xx, yy, 'smoothingspline','SmoothingParam',.1);
        % 
        % xxx =linspace(min(xx),max(xx),1e3);
        % plot(xxx,fs(xxx),':','LineWidth',1)
    end
    legend off
end
fig2pdf(figure(n),['figure' num2str(n)])


%% Tweak fig 6 to include spline to guide the eye

n = 6;
plot_csv_data(6, 'no save');
figure(n);
h=gcf;
h.Position=[1,1,3.5,4.5];

for i = 1
    ax = nexttile(i);
    lines = ax.Children;

    for j = length(lines):-1:2
        lines(j).LineStyle = ':';

        % xx = lines(j).XData';
        % yy = lines(j).YData';
        % fs = fit(xx, yy, 'smoothingspline','SmoothingParam',1e-3);
        % 
        % xxx =linspace(min(xx),max(xx),1e3);
        % plot(xxx,fs(xxx),':','LineWidth',1)
    end
    legend off
end
fig2pdf(figure(n),['figure' num2str(n)])


%%
fig2pdf(figure(n),['figure' num2str(n)])