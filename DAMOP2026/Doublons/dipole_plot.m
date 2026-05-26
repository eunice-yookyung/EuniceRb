%% Dipole-dipole interaction potential


magnification = 4;

marker = '.';
tri_width = 2 * sqrt(magnification);
tri_height = tri_width * .5;
trix = (0 + [-1,-1,1,-1]) * tri_width;
triz = (0 + [1, -1,0, 1]) * tri_height;
cl = 190;
lw = 0.5 * magnification;
arrowLinewidth = 1 * magnification;
cmap_crop = 50;
col_annotation = 'k';
axesFontsize = 8 * magnification;
labelFontsize = 7 * magnification;
xt = -40:40:40;
yt = xt;

colors = load('my_colors.mat','colormap_blue_red').colormap_blue_red;
colors([1:cmap_crop,end-cmap_crop+1:end],:) = [];
colors = flipud(colors);
% colors = diverging_cmap(colors(1,:), colors(end,:), 256);

xmax = 50 * 1e-9;
x=linspace(-xmax,xmax,5e2);
z=linspace(-xmax,xmax,5e2);
[X,Z]=meshgrid(x,z);
R=sqrt(X.^2+Z.^2);

Theta=acos(Z./R);

% Constants
hbar = 1.05e-34;
gamma = 6;
wavelen = 671e-9;
k=2*pi/wavelen;
C3 = 3/4*gamma;
detuning = 90;
om = 20 / gamma; % rabi freq in units of gamma

% dd potential
Vdd=3/4 * gamma ./(k*R).^3.*(1-3/2*sin(Theta).^2); % dipole-dipole interaction

% V = rot90(Vdd);
V = Vdd;

% Contour lines

scale = 5; % Density of lines. Smaller scale = more dense.
nlines = 1;
clines = 90 * ones(nlines,1);

for i = 2:nlines
    clines(i) = clines(i-1)*scale;
end
% clines = [0; clines];
clines = unique([-clines, clines]);
c=contour(X*1e9,Z*1e9,V,clines);
close

% Colormap
% cmap = zeros(256,3);
% cmap(:,3) = linspace(1,256,256)*.95;
% cmap(:,2) = linspace(0,256,256)*.8;
% cmap(:,1) = flip(1:256) * .9;
% cmap = cmap / 256;
cmap = colors;

% Plotting
figure('Units','inches','Position',[1,1,3.5/2 * magnification,3.5/2 * magnification])
s = surface(x*1e9,z*1e9,V,'EdgeColor','none','FaceAlpha',1);
h = get(s,'ZData');
set(s,'ZData',h-1e6)
clim([-1,1]*cl)
hold on

cl_skip  = 5;
for i = 1:length(clines)
    plot(c(1,1:cl_skip:end),c(2,1:cl_skip:end),marker,'MarkerSize',.05 * magnification,'Color',col_annotation)
end

% arrow
plot([1,1]*-35,[-40,-20],'r','linewidth', arrowLinewidth)
patch(+triz - 35,trix - 20, 'r','edgecolor','none')

% z line
plot([0,0],[min(z),max(z)] * 1e9,'--','Color',col_annotation,'linewidth',lw)

% theta line and angle label
th = 30 / 180*pi; % theta deg in rad
xth = 50 * sin(th);
yth = 50 * cos(th);
thlist = linspace(0,th,100);
xc = 20 * sin(thlist);
yc = 20 * cos(thlist);
plot([0,xth],[0,yth],'--','Color',col_annotation,'linewidth',lw)
plot(xc,yc,'-','Color',col_annotation,'linewidth',lw)
text(mean(xc)-.5,mean(yc)+5,mychar('theta'),'FontSize',labelFontsize,'Color',col_annotation)

pbaspect([1,1,1])
colormap(cmap)

cb = colorbar('Ticks',-200:100:200);
title(cb,'Shift (MHz)')


xlabel('x (nm)')%,'Position',[length(x)/2,length(z)+10])
ylabel('z (nm)')%,'Position',[-10,length(z)/2]);
set(gca,'FontSize',axesFontsize,'FontName','Segoe UI Symbol')
xticks(xt)
yticks(yt)
xlim([min(x),max(x)]*1e9)
ylim([min(z),max(z)]*1e9)
box on
% set(gca,'LineWidth',1)


%% Single dipolar potential

ms = 1;
marker = '-k';
lw = 1.5;
r0 = 1.2f;

xl = 2;
yl = 3.5;

npts = 1e4;
theta = linspace(0,1,npts) * 2*pi;

interpRangeRight = unique(round((npts/8:3*npts/8)));
interpRangeLeft = unique(round((5*npts/8:7*npts/8)));

fieldLinePositions = {0, 0, [-1,1],[-1.75,1.75], [-2.5,2.5]};

tri_width = 0.1;
tri_height = tri_width * 2;
trix = (0 + [0,-1,1,0]) * tri_width;
triz = (.3 + [-1, 1,1, -1]) * tri_height;

disp('---')

figure
for i = 1:5
    r0 = r0 * 1.5^(i-1);
    r = r0 * sin(theta).^2;
    xx = r .* sin(theta);
    zz = r .* cos(theta);
    % thetaRange = theta>pi+.1*pi&theta<237/180*pi;
    plot(xx,zz,'-k','markersize',ms,'linewidth',lw)
    hold on
    % plot(xx(interpRangeRight),zz(interpRangeRight),'.r','markersize',ms)
    hold on

    % Arrow positions
    f = fieldLinePositions{i};

    % disp(f)

    % Draw arrows
    for quadrant = 1:6

        if i > 2
            switch quadrant
                case 1
                    ii = theta > 0.01*pi & theta < pi/4;
                case 2
                    ii = theta > 3*pi/4 & theta < pi;
                case 3
                    ii = theta > pi+.1 & theta < 237/180*pi;
                case 4
                    ii = theta > 3*pi/4+pi & theta < 2*pi;
                otherwise
                    continue
            end
        else
            switch quadrant
                case 1
                    continue %ii = theta > 0.01*pi & theta < pi/4;
                case 2
                    continue %ii = theta > pi/2*1.5 & theta < pi;
                case 3
                    continue %ii = theta > pi+.1 & theta < 237/180*pi;
                case 4
                    continue % ii = theta > 3*pi/4+pi & theta < 2*pi;
                case 5
                    ii = theta > pi/2 - .1 & theta < pi/2+.1;
                case 6
                    ii = theta > 3*pi/2 - .1 & theta < 3*pi/2 + .1;
            end
        end

        for k = 1:length(f)
            z0 = f(k);
            zRange = zz(ii);
            xRange = xx(ii);
            x0 = interp1(zRange,xRange,z0);
            xp = x0 + trix;
            zp = z0 + triz;
            % patch(xp,zp,'k')
            % plot(x0,z0,'ok','linewidth',1)

            if ~isnan(x0) && ~isnan(z0)
                [~, iNearest] = min(abs(xRange-x0));
                zNear = zRange(iNearest-2:iNearest+2);
                xNear = xRange(iNearest-2:iNearest+2);

                % plot(xNear,zNear,'.r','MarkerSize',10)

                dx = mean(diff(xNear));
                dz = mean(diff(zNear));

                angle = atan(dz/dx);% + pi*(all(theta(ii))>pi);
                angle = mod(angle,pi) - pi/2;
                % angle = 2*pi-0.33*pi*2;
                if angle < -1e-3
                    angle = angle + pi;
                end
                if x0 > 0 && z0 < 0
                    angle = angle+pi;
                end
                if x0 < 0 && z0 > 0
                    angle = angle+pi;
                end

                R = [cos(angle) -sin(angle); sin(angle) cos(angle)];

                xt = trix;
                zt = triz;

                % Apply rotation
                XZ_rot = R * [xt; zt];

                x_rot = XZ_rot(1,:) + x0;
                z_rot = XZ_rot(2,:) + z0;

                % Draw rotated patch

                patch(x_rot, z_rot, 'k');


            end




        end


    end

end
%
plot(zeros(1e4,1),linspace(-1,1,1e4)*100,marker,'markersize',ms,'linewidth',lw)
patch(trix, -triz + 3, 'k')
patch(trix, -triz - 3, 'k')
xlim([-1,1]*xl)
ylim([-1,1]*yl)
pbaspect([xl,yl,1])

axis off

%%
Vd = cos(Theta)./R.^2;

V = Vd;

% Contour lines

scale = 5; % Density of lines. Smaller scale = more dense.
nlines = 3;
clines = 0.5 * ones(nlines,1);

for i = 2:nlines
    clines(i) = clines(i-1)*scale;
end
clines = [0; clines];
clines = unique([-clines, clines]);
c=contour(V,clines);
close

% Colormap
cmap = zeros(256,3);
cmap(:,3) = linspace(1,256,256);
cmap(:,2) = linspace(0,256,256);
cmap(:,1) = flip(1:256);
cmap = cmap / max(max(cmap));

% Plotting
figure
imshow(V,[-1,1]*4)
hold on

% for i = 1:length(clines)
plot(c(1,:),c(2,:),'k','MarkerSize',.1)
% end

pbaspect([1,1,1])
colormap(cmap)

colorbar
