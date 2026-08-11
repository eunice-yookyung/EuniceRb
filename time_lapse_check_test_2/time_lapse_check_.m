function bad_name_ids = time_lapse_check_(file_dir, time_thresh, plot_times)

if nargin < 2
    time_thresh = 40;
end
if nargin < 3
    plot_times = false;
end

prefix = 'scan';
suffix = '.fits';
listing = dir(fullfile(file_dir, [prefix '*' suffix]));
n_listing = length(listing);
dates = datetime({listing.date}');
time_diff_list = [0; seconds(diff(dates))]; % the zero is prepended to be consistent with the previous definition of time_diff_list.

long_time_lapse_index = find(time_diff_list > time_thresh); % set the time difference to look for in s

if plot_times
    figure('Position',[100,100,800,300])
    plot(dates, time_diff_list, 'o-', 'linewidth', 2)
    grid on
    xlabel('Time'),ylabel('Time difference (s)')
end

disp('Possible skipped shots around: ')
if ~isempty(long_time_lapse_index)

    % for each of the bad file name ranges, collect the names
    bad_name_ids = cell(length(long_time_lapse_index),1);

    for i = 1:length(long_time_lapse_index)

        idx_skipped = long_time_lapse_index(i);
        idx_rename = idx_skipped - 1;

        num_files = n_listing - (idx_rename) + 1;

        if plot_times
            text(dates(idx_rename), time_diff_list(idx_rename), listing(idx_rename).name, ...
                'BackgroundColor', 'none')
        end

        fprintf('\n[Range %d] %s.\nTime Lapse: %d seconds. \n%s to %s should go in bad_name_files (%d files).\n', ...
            i,...
            listing(idx_skipped).name, ...
            time_diff_list(idx_skipped), ...
            listing(idx_rename).name, ...
            listing(end).name, ...
            num_files)


        bad_name_ids{i} = string({listing(idx_rename:n_listing).name})';
        bad_name_ids{i} = char(bad_name_ids{i});
        bad_name_ids{i} = remove_pattern(bad_name_ids{i}, prefix);
        bad_name_ids{i} = remove_pattern(bad_name_ids{i}, suffix);

    end
else
    bad_name_ids = {};
    disp('None')
end
end


% =========================================================================
% Helper functions
% =========================================================================
% Remove pattern from string
% Input must be a character array
function out = remove_pattern(str, pat)

[idx_start, idx_end] = regexp(string(str), pat);
idx_start = unique(cell2mat(idx_start));
idx_end = unique(cell2mat(idx_end));


if isempty(idx_start) || isempty(idx_end)
    disp('No pattern detected. Returning original string.')
    out = str;
    return
elseif ~isscalar(idx_start) || ~isscalar(idx_end)
    disp('Error: different file name conventions detected.')
    out = str;
    return
else
    out = str(:,[1:idx_start-1,idx_end+1:end]);
end

end