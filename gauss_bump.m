x = -10:.01:10;
w = 0.75;
a = 0.03;
y = @(x) exp(-(x-2).^2/w^2) + exp(-(x+2).^2/w^2);
% bump = @(x) sqrt(a) * exp(-(x).^2/.82^2);
bump = @(x) sqrt(a) * exp(-(x).^2/.75^2);

Iy = @(x) abs(y(x)).^2;
Ib = @(x) abs(bump(x)).^2;

figure
plot(x, Iy(x))
hold on
plot(x, Ib(x))
plot(x, Iy(x) + Ib(x), '-k', 'linewidth', 2)

Iy(-1:1)
Ib(-1:1)
Iy(-1:1) + Ib(-1:1)