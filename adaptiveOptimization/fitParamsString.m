function out = fitParamsString(fobj)
% fitParamsString  Return fit formula and parameters +/- one sigma.
%
% Example output:
%   function = A*x + b
%   A = 1.23 \pm 0.04
%   b = 0.56 \pm 0.02
%
% Works for Curve Fitting Toolbox fit objects such as cfit/sfit.
%
% Note: one-sigma errors are estimated from the 68.27% confidence interval.

    % Get formula
    try
        fstr = formula(fobj);
    catch
        fstr = char(fobj);
    end

    % Get coefficient names and values
    names = coeffnames(fobj);
    vals  = coeffvalues(fobj);

    % Estimate 1-sigma uncertainties from 68.27% confidence intervals
    try
        ci = confint(fobj, 0.6827);   % approximately +/- 1 sigma
        errs = (ci(2,:) - ci(1,:)) / 2;
    catch
        warning('Could not compute confidence intervals. Returning NaN errors.');
        errs = nan(size(vals));
    end

    % Build output string
    lines = strings(numel(names) + 1, 1);
    lines(1) = "function = " + string(fstr);

    for k = 1:numel(names)
        lines(k+1) = sprintf('%s = %.3g \\pm %.3g', ...
            names{k}, vals(k), errs(k));
    end

    out = strjoin(lines, newline);
end