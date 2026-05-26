function cmap = diverging_cmap(c1, c2, N)
% DIVERGING_CMAP Create a diverging colormap from c1 -> black -> c2
%
%   cmap = diverging_cmap(c1, c2, N)
%
%   Inputs:
%     c1 : 3x1 or 1x3 RGB vector for the first color  (row 1)
%     c2 : 3x1 or 1x3 RGB vector for the last color   (row N)
%     N  : number of colors in the colormap (N x 3)
%          row 1         = c1
%          row round(N/2)= [0 0 0] (black)
%          row N         = c2
%
%   Notes:
%     - All color values are clipped to [0, 1].
%     - N must be >= 3.

    % Defaults
    if nargin < 3
        N = 64;
    end

    if N < 3
        error('N must be at least 3 to include c1, black center, and c2.');
    end

    % Ensure row vectors
    c1 = c1(:).';
    c2 = c2(:).';

    % Basic input checks
    if numel(c1) ~= 3 || numel(c2) ~= 3
        error('c1 and c2 must each have 3 elements (RGB).');
    end

    % Enforce [0, 1] range (clip)
    c1 = max(0, min(1, c1));
    c2 = max(0, min(1, c2));

    c0 = [0 0 0];  % black center

    mid = round(N/2);

    % Number of colors from c1 to black (including black)
    n1 = mid;
    % Number of colors from black to c2 (including black)
    n2 = N - mid + 1;

    % Interpolate c1 -> black
    t1 = linspace(0, 1, n1).';  % 0 -> 1
    seg1 = (1 - t1) * c1 + t1 * c0;

    % Interpolate black -> c2
    t2 = linspace(0, 1, n2).';
    seg2 = (1 - t2) * c0 + t2 * c2;

    % Combine, avoiding double black at the join
    cmap = [seg1; seg2(2:end, :)];

    % Final safety clip (floating point wonkiness)
    cmap = max(0, min(1, cmap));
end
