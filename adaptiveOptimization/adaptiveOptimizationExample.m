% Known / assumed model:
% y(f) = A * exp(-(f - f0)^2 / (2*sigma^2)) + noise

close all

n_rand = 22;
rng(n_rand);   % seed for reproducibility
plot_results = 0;

% =========================================================================
% Parameters
% =========================================================================
f0_true     = rand;    sigma_true = 1; % true signal parameters
noise_sigma = .3;                  % SNR ~3 at peak
maxIter     = 100;                  % number of adaptive steps
plot_every  = round(maxIter / 5);  % plot fit every N iterations

f_ideal = (-3:0.01:3)' + f0_true;
y_ideal = measure_signal(f_ideal, f0_true, sigma_true, 0);

% =========================================================================
% Run FIXED scan first (for convergence comparison)
% =========================================================================
% f_fixed_grid = (-5:0.5:5)'; 
f_fixed_grid = linspace(-4, 4, 50)';
f_fixed_grid = repmat(f_fixed_grid, ceil(maxIter / length(f_fixed_grid)));
f_fixed_grid = f_fixed_grid(1:maxIter+4);
rng(n_rand);
y_fixed_all  = measure_signal(f_fixed_grid, f0_true, sigma_true, noise_sigma);

n_fixed       = length(f_fixed_grid);
f0_err_fixed  = nan(n_fixed, 1);
p_hat_fixed   = nan(n_fixed, 4);   % store full fit at every fixed-scan step

for k = 3:n_fixed
    fd = f_fixed_grid(1:k);
    yd = y_fixed_all(1:k);

    % Skip until data spans both sides of the peak (within 1 sigma);
    % fitting from only one flank is genuinely ill-conditioned.
    if max(fd) < f0_true + sigma_true || min(fd) > f0_true - sigma_true
        continue
    end

    ph               = robust_fit(fd, yd, noise_sigma);
    p_hat_fixed(k,:) = ph;
    f0_err_fixed(k)  = abs(ph(2) - f0_true);
end

% =========================================================================
% Run ADAPTIVE scan
% =========================================================================
rng(n_rand);
f_data  = [-2; -1; 0; 1; 2];
y_data  = measure_signal(f_data, f0_true, sigma_true, noise_sigma);

% Pre-allocate convergence tracking
f0_err_adaptive = nan(maxIter, 1);
n_pts_adaptive  = nan(maxIter, 1);

if plot_results
    figure('Position', [100, 100, 550, 750], 'Name', 'Adaptive sampling')
    tiledlayout(3, 1)
end
for iter = 1:maxIter

    % ---- 1. Fit ----
    p_hat     = robust_fit(f_data, y_data, noise_sigma);

    A_hat     = p_hat(1);
    f0_hat    = p_hat(2);
    sigma_hat = abs(p_hat(3));
    b_hat     = p_hat(4);

    % ---- 2. Hessian-based uncertainty ----
    cost_fun = @(p) sum((y_data - gaussian_model(f_data, p)).^2 / noise_sigma^2);
    H      = finite_diff_hessian(cost_fun, p_hat);
    p_cov  = inv(H);
    f0_std = sqrt(max(p_cov(2,2), 0));

    % ---- 3. Log convergence ----
    f0_err_adaptive(iter) = abs(f0_hat - f0_true);
    n_pts_adaptive(iter)  = length(f_data);

    % ---- 4. Choose next frequency ----
    f_candidates = linspace(f0_hat - 2*sigma_hat, f0_hat + 2*sigma_hat, 201);

    y_pred            = A_hat * exp(-(f_candidates - f0_hat).^2 / (2*sigma_hat^2));
    sensitivity_to_f0 = abs(y_pred .* (f_candidates - f0_hat) / sigma_hat^2);

    if mod(iter, 2) == 1
        allowed = f_candidates < f0_hat;
    else
        allowed = f_candidates > f0_hat;
    end
    score = sensitivity_to_f0;
    score(~allowed) = 0;

    for k = 1:length(f_candidates)
        d = min(abs(f_candidates(k) - f_data));
        score(k) = score(k) * (1 - exp(-d^2 / (2*0.3^2)));
    end

    if max(score) == min(score)
        best_idx = ceil(rand * length(score));
    else
        [~, best_idx] = max(score);
    end
    f_next = f_candidates(best_idx);

    % ---- 5. Measure and add ----
    y_next = measure_signal(f_next, f0_true, sigma_true, noise_sigma);
    f_data = [f_data; f_next];
    y_data = [y_data; y_next];

    fprintf('Iter %3d | f_next = %+.3f | f0_hat = %+.3f | f0_std = %.4f\n', ...
            iter, f_next, f0_hat, f0_std);

    % ---- 6. Plot every plot_every iterations ----
    if plot_results && (mod(iter, plot_every) == 0 || iter == 1)

        y_fit_all = gaussian_model(f_data, p_hat);
        residuals = (y_data - y_fit_all) / noise_sigma;

        fit_str = sprintf('A=%.2f, f_0=%.3f±%.3f, \\sigma=%.3f, b=%.2f', ...
                          A_hat, f0_hat, f0_std, sigma_hat, b_hat);

        nexttile(1), cla
        stem(1:length(residuals), residuals, 'filled', 'MarkerSize', 4)
        yline(0, '-k', 'LineWidth', 1)
        yline( 1, '--', 'Color', [.6 .6 .6])
        yline(-1, '--', 'Color', [.6 .6 .6])
        ylabel('Residual / \sigma_{noise}')
        xlabel('Point index')
        title(sprintf('Normalized residuals (iter %d / %d)', iter, maxIter))

        nexttile(2), cla
        plot(f_candidates, score, '.-')
        xline(f0_hat, '--r', 'f_0', 'LabelVerticalAlignment', 'bottom')
        xlabel('Frequency'), ylabel('Score')
        title('Candidate score')

        nexttile(3), cla
        plot(f_ideal, y_ideal, '-', 'LineWidth', 2, 'Color', [1 1 1]*0.8, 'DisplayName', 'actual')
        hold on

        % --- Adaptive fit ---
        scatter(f_data, y_data, 'filled', 'MarkerFaceAlpha', 0.5, ...
                'MarkerFaceColor', 'r', 'DisplayName', 'adaptive data')
        plot(f_ideal, gaussian_model(f_ideal, p_hat), '-r', 'LineWidth', 1.5, 'DisplayName', fit_str)
        y_upper = gaussian_model(f_ideal, [A_hat, f0_hat + f0_std, sigma_hat, b_hat]);
        y_lower = gaussian_model(f_ideal, [A_hat, f0_hat - f0_std, sigma_hat, b_hat]);
        fill([f_ideal; flipud(f_ideal)], [y_upper; flipud(y_lower)], ...
             'r', 'FaceAlpha', 0.1, 'EdgeColor', 'none', 'DisplayName', 'adaptive 1\sigma')

        % --- Fixed scan fit at matching point count ---
        % iter-th adaptive step has (5 + iter) total points; match that in
        % the fixed grid (clamped to n_fixed).
        k_match = min(5 + iter, n_fixed);
        ph_fix  = p_hat_fixed(k_match, :);
        if ~any(isnan(ph_fix))
            fd_shown  = f_fixed_grid(1:k_match);
            yd_shown  = y_fixed_all(1:k_match);
            fix_str   = sprintf('fixed f_0=%.3f', ph_fix(2));
            scatter(fd_shown, yd_shown, 'filled', 'MarkerFaceAlpha', 0.3, ...
                    'MarkerFaceColor', 'b', 'DisplayName', 'fixed data')
            plot(f_ideal, gaussian_model(f_ideal, ph_fix), '--b', 'LineWidth', 1.5, ...
                 'DisplayName', fix_str)
        end

        xlabel('Frequency'), ylabel('Signal')
        title(sprintf('Iter %d / %d  |  f_{next} = %.3f', iter, maxIter, f_next))
        legend('Location', 'best')

        drawnow
        pause(0.3)
    end
end

% =========================================================================
% Convergence comparison figure
% =========================================================================
figure('Position', [700, 100, 600, 420], 'Name', 'Convergence comparison')

% Fixed scan: x-axis is number of points used (5 to n_fixed)
n_fixed_vec = (5:n_fixed)';
plot(n_fixed_vec, f0_err_fixed(5:end), ...
     '.-', 'Color', [0,0,1] + (1-[0,0,1])*.8, 'LineWidth', 1.5, 'MarkerSize', 6, 'DisplayName', 'Fixed scan')
hold on

plot(n_fixed_vec, movmean(f0_err_fixed(5:end), 10), ...
     'b.-', 'LineWidth', 1.5, 'MarkerSize', 6, 'DisplayName', 'Fixed scan, averaged')

% Adaptive: x-axis is total points in dataset at each iteration
plot(n_pts_adaptive, f0_err_adaptive, ...
     '.-', 'Color', [1,0,0] + (1-[1,0,0])*.8, 'LineWidth', 1.5, 'MarkerSize', 6, 'DisplayName', 'Adaptive')
plot(n_pts_adaptive, movmean(f0_err_adaptive, 10), ...
    'r.-', 'LineWidth', 1.5, 'MarkerSize', 6, 'DisplayName', 'Adaptive, averaged')

% Reference 1/sqrt(N) line anchored at first shared point
N_ref  = (5:max(n_pts_adaptive))';
% anchor = median(f0_err_fixed(5:30), 'omitnan');   % anchor to early fixed-scan performance
plot(N_ref, 1 * sqrt(5) ./ sqrt(N_ref), ...
     'k--', 'LineWidth', 1, 'DisplayName', '1/\surdN reference')

set(gca, 'YScale', 'log', 'XScale', 'log')
xlabel('Number of measurements')
ylabel('|f_0^{hat} - f_0|  (log scale)')
title('Convergence: adaptive vs fixed scan')
legend('Location', 'best')
grid on

% =========================================================================
% Helper functions
% =========================================================================
function p_hat = robust_fit(f, y, noise_sigma)
    % Grid search over [A, f0, sigma] to find global basin, then refine.
    A_grid     = [0.5, 1.0, 1.5];
    f0_grid    = linspace(min(f), max(f), 25);
    sigma_grid = [0.5, 1.0, 1.5, 2.0];
    b_grid     = [0, min(y)];

    best_cost = inf;
    best_p    = [1, mean(f), 1, 0];

    for A  = A_grid
    for f0 = f0_grid
    for sg = sigma_grid
    for b  = b_grid
        p   = [A, f0, sg, b];
        c   = sum((y - gaussian_model(f, p)).^2 / noise_sigma^2);
        if c < best_cost
            best_cost = c;
            best_p    = p;
        end
    end, end, end, end

    % Refine from best grid point
    cf    = @(p) sum((y - gaussian_model(f, p)).^2 / noise_sigma^2);
    opts  = optimset('MaxFunEvals', 5000, 'MaxIter', 2000, ...
                     'TolFun', 1e-8, 'TolX', 1e-8, 'Display', 'off');
    p_hat = fminsearch(cf, best_p, opts);
end


function y = gaussian_model(f, p)
    A     = p(1);
    f0    = p(2);
    sigma = abs(p(3));
    b     = p(4);
    y = A * exp(-(f - f0).^2 / (2*sigma^2)) + b;
end

function y = measure_signal(f, f0, sigma, noise_sigma)
    y = exp(-(f - f0).^2 / (2*sigma^2));
    y = y + randn(size(f)) * noise_sigma;
end

function H = finite_diff_hessian(f, x)
    n   = length(x);
    H   = zeros(n);
    eps = 1e-4;
    f0v = f(x);
    for i = 1:n
        for j = i:n
            ei = zeros(1, n); ei(i) = eps;
            ej = zeros(1, n); ej(j) = eps;
            if i == j
                H(i,i) = (f(x+ei) - 2*f0v + f(x-ei)) / eps^2;
            else
                H(i,j) = (f(x+ei+ej) - f(x+ei-ej) - f(x-ei+ej) + f(x-ei-ej)) / (4*eps^2);
                H(j,i) = H(i,j);
            end
        end
    end
end