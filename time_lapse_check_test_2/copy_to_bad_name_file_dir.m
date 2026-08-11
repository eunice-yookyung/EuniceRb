% Copy misnamed files to 'bad_name_files' subfolder
% function copy_to_bad_name_file_dir(file_dir, bad_name_ids, file_suffix)

% Automatically cut-paste all suffix if not provided
% if nargin < 3
    file_suffix = {'*.fits', '*.mat', '*atomMatrix.mat'};
% end
num_skipped_shots = length(bad_name_ids);
current_batch_len = 40;
target_batch_len = 40;

% Create the 'bad_name_files' directory if it doesn't exist
bad_name_dir = fullfile(file_dir, 'bad_name_files');
if ~exist(bad_name_dir, 'dir')
    fprintf('"bad_name_files" directory does not exist in %s. Creating the folder.\n', file_dir)
    mkdir(bad_name_dir);
end

% Ask for confirmation for each range
response = cell(num_skipped_shots,1);
for i = 1:num_skipped_shots
    n = size(bad_name_ids{i},1);
    response{i} = input(sprintf('Copy %d .mat files (%s to %s) [y/n]: ', n, bad_name_ids{i}(1,:), bad_name_ids{i}(end,:)), 's');
end

% Cut-paste files for each sequence
for i = 1:num_skipped_shots
    % If not changing that range of files, move on to the next range
    if ~strcmpi(response{i}, 'y')
        disp('Aborted. No files copied or deleted.')
        continue
    else % Cut-paste files
        for j = 1:length(file_suffix)
            bad_ids = bad_name_ids{i};
            d = dir(fullfile(file_dir, file_suffix{j}));
            files = {d.name}';
            if strcmp(file_suffix{j}, '*.mat')
                keep_idx = cellfun(@(s) ~contains(s, 'atomMatrix'), files);
                files = files(keep_idx);
            elseif strcmp(file_suffix{j}, '*atomMatrix.mat')
                keep_idx = cellfun(@(s) ~contains(s, 'meanAtomMatrix'), files);
                files = files(keep_idx);
            end

            % Of the existing files, find files with bad indices
            a = arrayfun(@(b) contains(files,b), string(bad_ids), 'UniformOutput', false);
            bad_file_idx = cell2mat(a');
            bad_file_idx = any(bad_file_idx,2);
            bad_files = files(bad_file_idx);
            n_bad_files = sum(bad_file_idx(:));

            fprintf('Renaming %d files (%s to %s)\n', n_bad_files, bad_files{1}, bad_files{end})
            w = waitbar(0, sprintf('Renaming 0/%d files', n_bad_files));
            for b = 1:n_bad_files
                waitbar(b/num_skipped_shots, w, sprintf('Renaming %d/%d files', b, num_skipped_shots))

                % Copying to bad_name_files and deleting old files
                copyfile(fullfile(file_dir,bad_files{b}),bad_name_dir)
                new_file_name = get_new_filename(bad_files{b}, bad_ids(b,:), current_batch_len, target_batch_len);
                % delete(fullfile(file_dir,bad_files{b}))

                % Copying old bad file name to original file
            end
            close(w)
        end
    end
end


% function new_filename = get_new_filename(old_filename, current_batch_len, target_batch_len)
%%

current_batch_len = 40;
target_batch_len = 40;
id = bad_ids(2,:);

[idx_start, idx_end] = regexp(old_filename, id);
letters = old_filename(idx_start:idx_start+1);
numbers = old_filename(idx_start+2:idx_end);

[A,B] = meshgrid(1:26, 1:26);
N = arrayfun(@(c) sprintf('%03d', c), (1:current_batch_len)', 'UniformOutput', false);
L = arrayfun(@(a,b) [char(a+65-1) char(b+65-1)], A(:), B(:), 'UniformOutput', false);

idx_letter = find(strcmp(L, letters));
idx_number = find(strcmp(N, numbers));

if current_batch_len == target_batch_len
    if idx_number < target_batch_len
        new_filename = [old_filename(1:idx_start-1) L{idx_letter} N{idx_number+1} old_filename(idx_end+1:end)];
    elseif idx_number == target_batch_len
        new_filename = [old_filename(1:idx_start-1) L{idx_letter+1} N{mod(idx_number, target_batch_len)+1} old_filename(idx_end+1:end)];
    end
end
% N = 
% end
