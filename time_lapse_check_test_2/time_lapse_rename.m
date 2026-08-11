% Find the files which indicate the experiment skipped running a shot.
% After finding those, ask user if they want to rename the chosen files.
% If user confirms, rename the files in the correct folders.

function data_all = time_lapse_rename(current_data_dir, target_batch_len, suffix_list, prefix)

time_thresh_s = 40; % If time between shots exceeds this amount, categorize as a skipped shot.
plot_times    = 0; % For diagnostics

% Default to current batch length if not provided or if empty
if nargin < 2
    target_batch_len = [];
end

% Default to all suffix types if not provided
if nargin < 3
    suffix_list = {'*.fits', '*.mat', '*atomMatrix.mat'};
end

% Default to 'scan' as a prefix if not provided
if nargin < 4
    prefix = 'scan';
end

% Find bad ids
bad_name_ids    = time_lapse_check(current_data_dir, time_thresh_s, plot_times);
data_all        = cell(length(suffix_list),2);

if isempty(bad_name_ids)
    disp('No bad files found. Aborting rename.')
    return
end

% User confirmation
response = input(sprintf('\nRename files (%s to %s)? [y/n]: ', bad_name_ids{1}(1,:), bad_name_ids{end}(end,:)), 's');

if ~strcmpi(response, 'y')
    disp('Aborted. No files renamed.')
    return
end

for s = 1:length(suffix_list)

    suffix = suffix_list{s};

    % Directory setup
    listing     = dir(fullfile(current_data_dir, suffix));
    filename    = string({listing.name}');

    % Bad name file setup
    bad_name_dir = fullfile(current_data_dir, 'bad_name_files');
    if ~exist(bad_name_dir, 'dir')
        fprintf('"bad_name_files" directory does not exist in %s. Creating the folder.\n', current_data_dir)
        mkdir(bad_name_dir);
    end

    % Filter out correct file types
    if strcmp(suffix, '*.mat')
        keep_idx = cellfun(@(s) contains(s, prefix) & ~contains(s, 'atomMatrix'), filename);
        filename = filename(keep_idx);
    elseif strcmp(suffix, '*atomMatrix.mat')
        keep_idx = cellfun(@(s) contains(s, prefix) & ~contains(s, 'mean'), filename);
        filename = filename(keep_idx);
    end

    % Sort and process files
    % Splice letter and numbers
    filename        = sort(filename); % sort by name
    num_files       = numel(filename);
    idx             = (1:num_files)';
    [~, idx_end]    = regexp(filename(1), prefix);
    filename        = char(filename);
    idx_let         = idx_end+1:idx_end+2;
    idx_num         = idx_end+3:idx_end+5;
    let             = filename(:, idx_let);
    num             = filename(:, idx_num);

    % Automatically detect current batch length
    current_batch_len = max(str2double(string(num)));
    if isempty(target_batch_len)
        target_batch_len = current_batch_len;
    end

    % Get letter and number lists
    [A,B] = meshgrid(1:26, 1:26);
    N = arrayfun(@(c) sprintf('%03d', c), (1:current_batch_len)', 'UniformOutput', false);
    L = arrayfun(@(a,b) [char(a+65-1) char(b+65-1)], A(:), B(:), 'UniformOutput', false);

    % For debugging
    % idx_l = arrayfun(@(s) find(strcmp(L, s)), string(let));
    % idx_n = arrayfun(@(s) find(strcmp(N, s)), string(num));

    data = table(idx, filename, let, num);

    num_skipped_shots = length(bad_name_ids);
    is_bad = zeros(num_files,1);
    for ns = 1:num_skipped_shots
        a = arrayfun(@(b) contains(string(data.filename),b), string(bad_name_ids{ns}), 'UniformOutput', false);
        bad_idx = cell2mat(a');
        bad_idx = any(bad_idx,2);
        first_bad = find(bad_idx,1,'first');
        if ~isempty(first_bad)
            bad_idx(first_bad:end) = 1;
        end
        is_bad = is_bad + any(bad_idx,2);
    end

    is_bad_idx = find(is_bad);
    num_bad = numel(is_bad_idx);
    idx_new = idx + is_bad;

    % Add new values to the results table
    idx_l_new = ceil(idx_new/target_batch_len);
    idx_n_new = mod(idx_new-1,target_batch_len)+1;

    let_new = char(L(idx_l_new));
    num_new = char(N(idx_n_new));

    filename_new = filename;
    filename_new(:,idx_let) = let_new;
    filename_new(:,idx_num) = num_new;

    data = addvars(data,idx_new,'After',1);
    data = addvars(data,is_bad,'After','idx_new');
    data = addvars(data,filename_new,'After','num');
    data = addvars(data,let_new,'After','filename_new');
    data = addvars(data,num_new,'After','let_new');

    % Rename each bad file
    w = waitbar(0, sprintf('Renaming 0/%d files', num_bad));
    barLen = 30;
    fprintf(repmat(' ', 1, barLen+10))
    for b = 1:num_bad
        waitbar(b/num_bad, w, sprintf('Renaming %d/%d files', b, num_bad));
        b_idx = is_bad_idx(b);
        dat = data(b_idx,:);
        fn_old = dat.filename;
        fn_new = dat.filename_new;

        % Printing
        % fprintf('Copying %s to %s and deleting from %s\n',fn_old,bad_name_dir,file_dir)
        copyfile(fullfile(current_data_dir,fn_old),bad_name_dir) % Copy to bad name folder
        delete(fullfile(current_data_dir,fn_old)) % Delete from original folder
        copyfile(fullfile(bad_name_dir,fn_old),fullfile(current_data_dir,fn_new)) % Rename to new file

    end
    close(w)

    data_all{s,1} = suffix_list{s};
    data_all{s,2} = data; % Append data to the final cell output

end

end

% =========================================================================
% Helper functions
% =========================================================================

% Main skipped shot checker.
% If time elapsed between two shots exceeds time_thresh_s, categorize it as
% a 'skipped' shot.
function bad_name_ids = time_lapse_check(file_dir, time_thresh_s, plot_times)

if nargin < 2
    time_thresh_s = 40;
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

long_time_lapse_index = find(time_diff_list > time_thresh_s); % set the time difference to look for in s

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
