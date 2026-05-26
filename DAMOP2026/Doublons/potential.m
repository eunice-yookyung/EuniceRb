% Dipole dipole interaction potential drawing

% Plot parameters
r = linspace(1,200,1e3)*1e-9;
tri_width = .5; % arrowhead parameters
tri_height = tri_width * 2;
trix = (0 + [0,-1,1,0]) * tri_width;
triz = (1 + [-1, 1,1, -1]) * tri_height;
col_tri = [1,1,1]*.5;
x_detuning = 60; % position of detuning arrow
magnification = 1;
xl = [20,140]; % limits
yl = [-18,48];
xl_inset = [20,60] + [-1,1]*3;
yl_inset = [-1,1]*6;
xt_inset = 20:20:75;
yt_main = -15:15:90;
xt_main = 40:40:300;
yt_label = string(yt_main + 15);

% line styles
col_anticrossing = [1, .5, .5];
lw_anticrossing = 1;
ls_anticrossing = '-';
col_crossing = [0,0,0];
lw_crossing = lw_anticrossing;
ls_crossing = '--';


% constants
hbar = 1.05e-34;
gamma = 6;
wavelen = 671e-9;
k=2*pi/wavelen;
C3 = 3/4*gamma;
detuning = 90;
om = 20 / gamma; % rabi freq in units of gamma

% functions
f = @(x) (C3./(k*x).^3 - detuning)/ gamma;
g = @(x) zeros(size(x));
d = @(x) f(x) - g(x);
t = 0 * -detuning/gamma; % offset
r0 = interp1(f(r),r,0);
M1 = @(x) 1/2 * (f(x) + sqrt(om^2 + d(x).^2)); % generalized rabi frequency
M2 = @(x) 1/2 * (f(x) - sqrt(om^2 + d(x).^2)); % generalized rabi frequency

% plotting
figure('Units','inches','Position',[.5,.5,3.5*magnification,2*magnification])

plot(r * 1e9, f(r) - t,ls_crossing,'Color',col_crossing,'linewidth',lw_crossing),hold on
plot(r * 1e9, g(r) - t,ls_crossing,'Color',col_crossing,'linewidth',lw_crossing)
plot(r * 1e9, M1(r) - t,ls_anticrossing,'Color',col_anticrossing,'linewidth',lw_anticrossing)
plot(r * 1e9, M2(r) - t,ls_anticrossing,'Color',col_anticrossing,'linewidth',lw_anticrossing)
plot(xl, [1,1] * (-detuning/gamma) - t, ':k') % bare detuning
plot([1,1] * x_detuning,[-.9,-.1] * detuning/gamma-t,'Color',col_tri,'linewidth',lw_anticrossing)
patch(trix +  x_detuning, -triz-t, col_tri,'edgecolor','none')
patch(trix +  x_detuning, +triz-detuning/gamma-t, col_tri,'edgecolor','none')
text(x_detuning+1,-detuning/gamma/2-t,mychar('delta'),'FontSize',8,'Color',col_tri)

% labels and settings
xlim(xl),ylim(yl),pbaspect([1,.5,1])
set(gca,'FontSize',8,'FontName','Segoe UI Symbol')
xlabel('Axial distance (nm)'),ylabel(['Energy (' mychar('hbar') mychar('Gamma') ')'])
ax1 = gca;
yticks(yt_main)
yticklabels(yt_label)
xticks(xt_main)

ax_x1 = ax1.Position(1);
ax_y1 = ax1.Position(2);
ax_x2 = ax1.Position(3);
ax_y2 = ax1.Position(4);

% new axis: inset
% axes('Position',[(ax_x1 + ax_x2)/2,(ax_y1 + ax_y2)/2,ax_x2,ax_y2])
axes('Position',[.49,.45,ax_x2/2,ax_y2/2])

plot(r * 1e9, f(r) - t,ls_crossing,'Color',col_crossing,'linewidth',lw_crossing),hold on
plot(r * 1e9, g(r) - t,ls_crossing,'Color',col_crossing,'linewidth',lw_crossing)
plot(r * 1e9, M1(r) - t,ls_anticrossing,'Color',col_anticrossing,'linewidth',lw_anticrossing)
plot(r * 1e9, M2(r) - t,ls_anticrossing,'Color',col_anticrossing,'linewidth',lw_anticrossing)
plot(xl, [1,1] * (-detuning/gamma) - t, '--k') % bare detuning
% labels and settings
xlim(xl_inset),ylim(yl_inset),pbaspect([1,.5,1])
set(gca,'FontSize',8,'FontName','Segoe UI Symbol')
xticks(xt_inset)
ax1 = gca;