function [tileObj, data_plot, files_plot] = plot_csv_data_damop(files_to_plot, saveStr)
% PLOT_CSV_DATA  Plot CSV data for figures 2, 4, 5, and 6 of the paper.
%
%   [tileObj, data_plot, files_plot] = plot_csv_data(files_to_plot, saveStr)
%
%   Inputs:
%     files_to_plot  - Vector of figure numbers to plot (e.g. [4, 5] or [2, 4, 5, 6])
%     saveStr        - Set to 'save' to export figures as PDFs; any other
%                      value skips saving
%
%   Outputs:
%     tileObj        - Handle to the last tiledlayout object created
%     data_plot      - Cell array of data tables for fig 2 (empty if not plotted)
%     files_plot     - Cell array of filenames for fig 2 (empty if not plotted)

% =========================================================================
%% 1. LOAD ALL CSV FILES
% =========================================================================

dirname = 'paper_plots_data_hanzhen';
d = dir([dirname '/fig*.csv']);
filenames = {d.name}';
Nf = length(filenames);

% Read all CSVs into a cell array; suppress warnings about variable naming
data = cell(size(filenames));
warning off
for j = 1:Nf
    data{j} = readtable(fullfile(dirname, filenames{j}));
end
warning on
disp('All CSV files loaded.')

% =========================================================================
%% 2. FIGURE LAYOUT CONFIGURATION
% =========================================================================

% Figure numbers as they appear in the paper
filenums = [2, 4, 5, 6];

% Whether each figure is split into two panels (1 = yes, 0 = no)
panel_split = [1, 1, 1, 1];

% Panel arrangement: 0 = horizontal (1×2), 1 = vertical (2×1)
panel_orientation = [0, 0, 0, 0];

% Which panel label goes on the right side: 0 = upper-left, 1 = upper-right
panel_label_loc = [0, 1, 1, 1];

% Physical figure dimensions [width, height] in inches
% hor_fig_dim  = [3.5, 3.0];   % used for horizontal (1×2) layouts
hor_fig_dim  = [3.5*1, 2.50];   % used for horizontal (1×2) layouts
vert_fig_dim = [3.5, 4.5];   % used for vertical  (2×1) layouts
% vert_fig_dim = [2.5, 4.5];   % used for vertical  (2×1) layouts

% =========================================================================
%% 3. APPEARANCE / STYLE CONFIGURATION
% =========================================================================

% Font settings
axesFontSize  = 8;
labelFontsize = 7;
fontName      = 'Arial';

% Marker and error bar settings
markerSize = 10;
capSize    = 0;        % no caps on error bars

% Whether to overlay a smoothing-spline fit on each figure's panels
include_splinefit = [0, 0, 1, 1];

% --- Color palette ---
% Load a blue-red diverging colormap; reverse and darken the first 5 entries
colors = load('my_colors.mat').colors_blue_red;
colors(1:5,:) = colors(5:-1:1,:);
colors(1:5,:) = colors(1:5,:) * 0.9;

% Doublon/singlon theory and data colors
theory_col_doublons = [1, 0.5, 0.5];          % light red
% theory_col_doublons = [41, 171, 256] / 256;
data_col_doublons   = theory_col_doublons * 0.8;
theory_col_singlons = [1, 1, 1] * 0.2;        % dark gray
data_col_singlons   = theory_col_singlons * 0.2;

% Color order applied to each panel of each figure {fig}{panel}
color_index = { ...
    % Fig 2: theory/data for singlons and doublons
    {[theory_col_singlons; theory_col_doublons; data_col_doublons; data_col_singlons], ...
     [theory_col_singlons; theory_col_doublons; data_col_doublons; data_col_singlons]}, ...
    % Fig 4: 5 colors from the palette + singlon theory
    {[colors(1:5,:); theory_col_singlons], [colors(1:5,:); theory_col_singlons]}, ...
    % Fig 5: doublon and singlon data colors
    {[data_col_doublons; data_col_singlons], [data_col_doublons; data_col_singlons]}, ...
    % Fig 6: palette colors / doublon+singlon theory+data
    {colors(1:5,:), [theory_col_doublons; data_col_doublons; data_col_singlons]} ...
};

% =========================================================================
%% 4. AXIS LABELS, TITLES, AND LIMITS
% =========================================================================

% Keyword used to decide which panel (1 or 2) a file belongs to.
% Files containing this keyword → panel 1; all others → panel 2.
panel_sep_keyword = {{'axis0', 'data'}, 'radial', 'condon', 'ejected'};

% X-axis labels {fig}{panel}
xlabelstr = { ...
    {'Hole fraction'}, ...
    {['Radial distance (' mychar('mu') 'm)'], 'Relative distance (nm)'}, ...
    {['Pulse duration (' mychar('mu') 's)'], ['Pulse duration (' mychar('mu') 's)']}, ...
    {'Scattering length (nm)', 'Scattering length (nm)'} ...
};

% Y-axis labels {fig}{panel}
ylabelstr = { ...
    {'Normalized scattering', 'Normalized elastic scattering'}, ...
    {'Radial distribution', ''}, ...
    {'Resonantly ejected atoms', 'Normalized scattering'}, ...
    {'Resonantly ejected atoms', 'Normalized incoherent scattering'} ...
};

% Subplot titles {fig}{panel}
titlestr = { ...
    {''}, ...
    {'Afer time-of-flight', 'Simulation (in-situ)'}, ...
    {''}, ...
    {''} ...
};

% X and Y axis limits [panel1_lim; panel2_lim] for each figure (rows alternate)
xlimits_list = [ ...
    [0,   1;   0,   1  ]; ...  % Fig 2
    [0, 160;   0, 350  ]; ...  % Fig 4
    [0,  25;   0,  25  ]; ...  % Fig 5
    [-55, 55; -55,  55 ]  ...  % Fig 6
];

ylimits_list = [ ...
    [-.0, 1.2  ; -.05, 2.05]; ...  % Fig 2
    [-.05, 0.45; -.05, 1.05]; ...  % Fig 4
    [-.02, 0.42;    0, 1.60]; ...  % Fig 5
    [-.02, 0.32;    0, 2.00]  ...  % Fig 6
];

% Explicit tick positions {fig}{panel}
xticks_list = { ...
    {0:0.5:1,    0:0.5:1  }; ...  % Fig 2
    {0:75:160,   0:150:400}; ...  % Fig 4
    {0:5:25;     0:5:25   }; ...  % Fig 5
    {-50:25:50; -50:25:50 }  ...  % Fig 6
};

yticks_list = { ...
    {0:0.5:2,   0:0.5:2  }; ...  % Fig 2
    {0:0.2:1,   0:0.5:1  }; ...  % Fig 4
    {0:0.2:1;   0:0.5:1.6}; ...  % Fig 5
    {0:0.1:0.3; 0:0.5:2  }  ...  % Fig 6
};

% =========================================================================
%% 5. PLOT FIGURES 4, 5, AND 6  (fig 2 handled separately below)
% =========================================================================

close all

% Determine which figures to plot; always skip fig 2 in this loop
plot_on = ismember(filenums, files_to_plot);
plot_on(1) = 0;

tilenum = 1;   % tracks which panel tile is currently active

for i = 1:length(filenums)

    if ~plot_on(i)
        continue
    end

    n      = filenums(i);
    prefix = ['fig' num2str(n)];

    % --- Iterate over all loaded files and find those matching this figure ---
    for j = 1:Nf
        fn  = filenames{j};
        dat = data{j};

        if ~contains(fn, prefix)
            continue
        end

        % Choose line vs. dot marker based on dataset length
        if size(dat, 1) > 50
            marker = '-';
            include_splinefit(i) = 0;   % spline not meaningful for dense data
        else
            marker = '.:';
        end

        % Create figure window on first encounter; set up tiled layout
        if ~ishandle(n)
            h = figure(n);
            if panel_split(i)
                if panel_orientation(i)
                    % Vertical: two rows, one column
                    tileObj = tiledlayout(2, 1, 'TileSpacing', 'compact', 'Padding', 'compact');
                    set(h, 'Units', 'inches', 'Position', [1 + 3.5*(i-1), 1, vert_fig_dim]);
                else
                    % Horizontal: one row, two columns
                    tileObj = tiledlayout(1, 2, 'TileSpacing', 'compact', 'Padding', 'compact');
                    set(h, 'Units', 'inches', 'Position', [1 + 3.5*(i-1), 1, hor_fig_dim]);
                end
            end
        end
        h = figure(n);
        h.Color = 'w';

        % --- Determine which panel this file belongs to ---
        if panel_split(i)
            if contains(fn, panel_sep_keyword{i})
                tilenum = 1;
            else
                tilenum = 2;
            end
            nexttile(tilenum);
        end

        % --- Draw the data: errorbar for 3-column files, line plot otherwise ---
        if size(dat, 2) == 3 && any(contains(dat.Properties.VariableNames, 'err'))
            % Data with uncertainties
            errorbar(dat.(1), dat.(2), dat.(3), marker, ...
                'MarkerSize', markerSize, ...
                'CapSize',    capSize, ...
                'LineWidth',  1, ...
                'DisplayName', strrep(fn(6:end-4), '_', ' '));
            hold on

            % Optional smoothing-spline overlay
            if include_splinefit(i)
                fit(dat.(1), dat.(2), 'smoothingspline', 'SmoothingParam', 0.1);
            end
        else
            % Plain line data (fig 2 only plots first 3 columns)
            nlines = 3;
            if n ~= 2
                nlines = size(dat, 2);
            end
            for k = 2:nlines
                plot(dat.(1), dat.(k), marker, ...
                    'LineWidth',  1, ...
                    'MarkerSize', markerSize, ...
                    'DisplayName', strrep(fn(6:end-4), '_', ' '));
                hold on
            end
        end

        % --- Apply axis labels, title, limits, and ticks ---
        try, xlabel(xlabelstr{i}{tilenum}); catch, xlabel(xlabelstr{i}{1}); end
        try, ylabel(ylabelstr{i}{tilenum}); catch, ylabel(ylabelstr{i}{1}); end
        try,  title(titlestr{i}{tilenum});  catch,  title(titlestr{i}{1});  end

        xlim(xlimits_list(2*i - (2 - tilenum), :))
        ylim(ylimits_list(2*i - (2 - tilenum), :))
        xticks(xticks_list{i}{tilenum})
        yticks(yticks_list{i}{tilenum})

        set(gca, ...
            'FontSize',   axesFontSize, ...
            'Color',      'none', ...
            'ColorOrder', color_index{i}{tilenum}, ...
            'FontName',   fontName)
        legend off

    end % file loop

    % --- Add panel labels (a) / (b) to each tile ---
    for tiles = [1, 2]
        nexttile(tiles)
        panelstr = '(a)';
        if tiles == 2, panelstr = '(b)'; end

        xl    = xlim;
        yl    = ylim;
        ratio = pbaspect;
        ratiox = ratio(1)^0.25;
        ratioy = 0.95;
        if ratio(2) < 1, ratioy = 0.9; end

        if panel_label_loc(i)
            % Upper-right placement
            % text(xl(1) + diff(xl)*0.9*ratiox, yl(1) + diff(yl)*ratioy, panelstr, ...
            %     'FontSize', labelFontsize)
        else
            % Upper-left placement
            % text(xl(1) + diff(xl)*0.1, yl(1) + diff(yl)*ratioy, panelstr, ...
            %     'FontSize', labelFontsize)
        end
    end

    % --- Optionally save figure as PDF ---
    if strcmp(saveStr, 'save')
        fig2pdf(figure(n), ['figure' num2str(n)])
    end

end % figure loop

% =========================================================================
%% 6. SPECIAL HANDLING FOR FIGURE 2
%    Fig 2 uses a 5×2 tile grid with the data panels spanning 3 rows.
% =========================================================================

data_plot  = {};
files_plot = {};

plot_on = ismember(filenums, files_to_plot);
if ~plot_on(1)
    return
end

i      = 1;   % index into filenums for fig 2
n      = 2;
prefix = 'fig2';

for j = 1:Nf
    fn  = filenames{j};
    dat = data{j};

    if ~contains(fn, prefix)
        continue
    end

    % Collect fig-2 data for the caller
    data_plot{end+1}  = dat;   %#ok<AGROW>
    files_plot{end+1} = fn;    %#ok<AGROW>

    % Choose marker style by dataset size
    if size(dat, 1) > 50
        marker = '-';
        include_splinefit(i) = 0;
    else
        marker = '.';
    end

    % Create figure with a 5×2 tile grid on first encounter.
    % Data panels will span rows 3–5 (via nexttile with span argument).
    if ~ishandle(n)
        h = figure(n);
        if panel_split(i)
            tileObj = tiledlayout(5, 2, 'TileSpacing', 'compact', 'Padding', 'compact');
            set(h, 'Units', 'inches', 'Position', [1 + 3.5*(i-1), 1, 3.5, 4]);
        end
    end
    h = figure(n);
    h.Color = 'w';

    % Place this panel in tile 1 or 2 (spanning 3 rows) based on filename keyword
    if panel_split(i)
        if contains(fn, panel_sep_keyword{i})
            tilenum = 1;
        else
            tilenum = 2;
        end
        nexttile(4 + tilenum, [3, 1]);   % start at row 3, span 3 rows
    end

    % --- Draw data ---
    if size(dat, 2) == 3 && any(contains(dat.Properties.VariableNames, 'err'))
        errorbar(dat.(1), dat.(2), dat.(3), marker, ...
            'MarkerSize', markerSize, ...
            'CapSize',    capSize, ...
            'LineWidth',  1, ...
            'DisplayName', strrep(fn(6:end-4), '_', ' '));
        hold on
    else
        for k = 2:3   % fig 2 always plots columns 2 and 3 only
            plot(dat.(1), dat.(k), marker, ...
                'LineWidth',  1, ...
                'MarkerSize', markerSize, ...
                'DisplayName', strrep(fn(6:end-4), '_', ' '));
            hold on
        end
    end

    % --- Labels, limits, ticks ---
    try, xlabel(xlabelstr{i}{tilenum}); catch, xlabel(xlabelstr{i}{1}); end
    try, ylabel(ylabelstr{i}{tilenum}); catch, ylabel(ylabelstr{i}{1}); end
    try,  title(titlestr{i}{tilenum});  catch,  title(titlestr{i}{1});  end

    xlim(xlimits_list(2*i - (2 - tilenum), :))
    ylim(ylimits_list(2*i - (2 - tilenum), :))
    xticks(xticks_list{i}{tilenum})
    yticks(yticks_list{i}{tilenum})

    set(gca, ...
        'FontSize',   axesFontSize, ...
        'Color',      'none', ...
        'ColorOrder', color_index{i}{tilenum}, ...
        'FontName',   fontName)
    legend off

    % --- Panel label: (b) for panel 1, (c) for panel 2 ---
    if tilenum == 1
        panelstr = '(b)';
    else
        panelstr = '(c)';
    end

    xl    = xlim;
    yl    = ylim;
    ratio = pbaspect;
    ratiox = ratio(1)^0.25;
    ratioy = 0.95;
    if ratio(2) < 1, ratioy = 0.9; end

    if panel_label_loc(i)
        text(xl(1) + diff(xl)*0.9*ratiox, yl(1) + diff(yl)*ratioy, panelstr, ...
            'FontSize', labelFontsize)
    else
        text(xl(1) + diff(xl)*0.05, yl(1) + diff(yl)*ratioy, panelstr, ...
            'FontSize', labelFontsize)
    end

end % fig 2 file loop

end % function