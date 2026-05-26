% test

x = 0:.01:1.5;
yl = [-1,1]*5;

for om = .5

    lw = 1;
    f = @(x) 1./x.^3-5; % bare energy
    g = @(x) zeros(size(x)); % bare energy
    d = @(x) f(x) - g(x); % detuning
    
    
    quad_sum_1 = @(x) 1/2 * (f(x) + sqrt(om^2 + d(x).^2)); % generalized rabi frequency
    quad_sum_2 = @(x) 1/2 * (f(x) - sqrt(om^2 + d(x).^2)); % generalized rabi frequency
    
    
    % t = @(x) zeros(size(x)); % bare functions
    t = @(x) f(x); % set to zero level
    
    
    figure
    tiledlayout(1,2,'TileSpacing','compact','Padding','compact')
    nexttile
    plot(x,f(x),'--k','linewidth',lw)
    hold on
    plot(x,g(x),'--r','linewidth',lw)
    plot(x,quad_sum_1(x),'-k','linewidth',lw)
    plot(x,quad_sum_2(x),'-r','linewidth',lw)
    ylim(yl)
    
    nexttile
    plot(x,f(x) - t(x),'--k','linewidth',lw)
    hold on
    plot(x,g(x) - t(x),'--r','linewidth',lw)
    plot(x,quad_sum_1(x) - t(x),'-k','linewidth',lw)
    plot(x,quad_sum_2(x) - t(x),'-r','linewidth',lw)
    ylim(yl)

    disp(num2str(min(quad_sum_1(x))))
end