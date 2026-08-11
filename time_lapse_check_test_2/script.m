clear

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
file_dir            = '8_test';
prefix              = 'scan';
target_batch_len    = 5;
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

bad_name_ids = time_lapse_check(file_dir);

suffix_list = {'*.fits', '*.mat', '*atomMatrix.mat'};
for s = 1:length(suffix_list)

    suffix = suffix_list{s};

    % -------------------------------------------------------------------------
    % Directory setup
    listing     = dir(fullfile(file_dir, suffix));
    filename    = string({listing.name}');
    prefix      = 'scan';

    % Bad name file setup
    bad_name_dir = fullfile(file_dir, 'bad_name_files');
    if ~exist(bad_name_dir, 'dir')
        fprintf('"bad_name_files" directory does not exist in %s. Creating the folder.\n', file_dir)
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
    filename    = sort(filename); % sort by name
    num_files   = numel(filename);
    idx         = (1:num_files)';
    [~, idx_end] = regexp(filename(1), prefix);
    filename       = char(filename);
    idx_let        = idx_end+1:idx_end+2;
    idx_num        = idx_end+3:idx_end+5;
    let = filename(:, idx_let);
    num = filename(:, idx_num);

    % Automatically detect current batch length
    current_batch_len = max(str2double(string(num)));

    % Get letter and number lists
    [A,B] = meshgrid(1:26, 1:26);
    N = arrayfun(@(c) sprintf('%03d', c), (1:current_batch_len)', 'UniformOutput', false);
    L = arrayfun(@(a,b) [char(a+65-1) char(b+65-1)], A(:), B(:), 'UniformOutput', false);

    idx_l = arrayfun(@(s) find(strcmp(L, s)), string(let));
    idx_n = arrayfun(@(s) find(strcmp(N, s)), string(num));

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
    disp(data)


    for b = 1:num_bad
        b_idx = is_bad_idx(b);
        dat = data(b_idx,:);
        fn_old = dat.filename;
        fn_new = dat.filename_new;

        fprintf('Copying %s to %s and deleting from %s\n',fn_old,bad_name_dir,file_dir)
        % copyfile(fullfile(file_dir,fn_old),bad_name_dir)
        % delete(fullfile(file_dir,fn_old))

    end

end
