function out = rfplot_damop(tileObject)

% make the rf plot
% close all

siteRadius = 0.5;
singlonSiteColor = [1,1,1] * .7;
doublonSiteColor = [1,1,1] * .2;
singlonSiteLinewidth = .5;
doublonSiteLinewidth = 1;
singlonRadius = 4.4*.8;
doublonRadius = 2.5*0;
atomMarker = '.';
singlonMarkersize = 11;
% aColor = [1, .25, .25] * 1;
% aColor = [41, 171, 256] / 256;
aColor = [1, 0.5, 0.5] *0;

bColor = [1, 1, 1] * .9;
theta = (0:.01:1) * 2*pi;
xcirc = siteRadius * cos(theta);
ycirc = siteRadius * sin(theta);
doub_separation = .15;
xdoub = [-1,1] * doub_separation;
ydoub = [-1,1] * doub_separation;
tri_width = .5;
tri_height = tri_width * .5;
trix = (-1 + [-1,-1,1,-1]) * tri_width;
triz = (0 + [1, -1,0, 1]) * tri_height;
arrowColor = [0,0,0];
arrowLinewidth = 1.5;
after_rf_offset_x = 2 * ceil(singlonRadius) + 3;
holeFraction = .0;


xl = [-ceil(singlonRadius), ceil(singlonRadius) + after_rf_offset_x];
yl = [-1, 1] * ceil(singlonRadius);


if nargin < 1
    figure('Units','inches','Position',[1,1,3.5,3.5],'Color','w')
else
    nexttile(tileObject, 1, [2,2])
end

%%% before rf sites
% plot singlon sites
for x0 = round(-singlonRadius:(2*siteRadius):singlonRadius)
    for y0 = round(-singlonRadius:(2*siteRadius):singlonRadius)
        if x0^2 + y0^2 < singlonRadius^2 && x0^2 + y0^2 >= doublonRadius^2
            plot(x0 + xcirc,y0 + ycirc, 'Color', singlonSiteColor, 'LineWidth', singlonSiteLinewidth)
            hold on
            plot(x0, y0, atomMarker, ...
                'Color', aColor, 'MarkerSize', singlonMarkersize)
        end
    end
end
% plot doublon sites
for x0 = round(-singlonRadius:(2*siteRadius):singlonRadius)
    for y0 = round(-singlonRadius:(2*siteRadius):singlonRadius)
        if x0^2 + y0^2 < doublonRadius^2
            plot(x0 + xcirc,y0 + ycirc, 'Color', doublonSiteColor, 'LineWidth', doublonSiteLinewidth)
            hold on
            plot(x0 + xdoub,y0 + ydoub, atomMarker, ...
                'Color', aColor, 'MarkerSize', singlonMarkersize)
        end
    end
end


%%% after rf sites
% plot singlon sites
for x0 = round(-singlonRadius:(2*siteRadius):singlonRadius)
    for y0 = round(-singlonRadius:(2*siteRadius):singlonRadius)
        if x0^2 + y0^2 < singlonRadius^2 && x0^2 + y0^2 >= doublonRadius^2
            plot(x0 + xcirc + after_rf_offset_x,y0 + ycirc, 'Color', singlonSiteColor, 'LineWidth', singlonSiteLinewidth)
            hold on
            plot(x0 + after_rf_offset_x, y0, atomMarker, ...
                'Color', bColor, 'MarkerSize', singlonMarkersize)
        end
    end
end

% plot doublon sites
stays_a = rand(100, 1) < holeFraction;
idoub = 0;
for x0 = round(-singlonRadius:(2*siteRadius):singlonRadius)
    for y0 = round(-singlonRadius:(2*siteRadius):singlonRadius)
        if x0^2 + y0^2 < doublonRadius^2
            idoub = idoub + 1; % Increment doublon counter
            if stays_a(idoub)
                col = bColor;
            else
                col = aColor;
            end
            plot(x0 + xcirc + after_rf_offset_x,y0 + ycirc, 'Color', doublonSiteColor, 'LineWidth', doublonSiteLinewidth)
            hold on
            plot(x0 + xdoub + after_rf_offset_x,y0 + ydoub, atomMarker, ...
                'Color', col, 'MarkerSize', singlonMarkersize)
        end
    end
end


% plot rf arrow
plot([-1.5,1] + after_rf_offset_x/2, [0,0],'-','linewidth',arrowLinewidth,'Color',arrowColor)
patch(trix+[1]*1.5 + after_rf_offset_x/2,triz,arrowColor)

text(after_rf_offset_x/2-.7, 0+1, 'RF')
% text(diff(xl)*.93 + xl(1), diff(yl) * .93 + yl(1), '(a)', 'FontSize', 8)

xlim(xl)
ylim(yl)
pbaspect([diff(xl)/diff(yl), 1, 1])


xticks([])
yticks([])
axis off

end