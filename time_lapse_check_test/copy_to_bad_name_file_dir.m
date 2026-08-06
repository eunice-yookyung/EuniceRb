% Copy misnamed files to 'bad_name_files' subfolder

% function copy_to_bad_name_file_dir(fileDir, bad_name_files)

% for testing only
% if nargin < 1
file_dir = 'Copy_of_9_rabi_oscillation_106107_EpU_EmU_parity_proj_long_times';
% end
num_skipped_shots = length(bad_name_files);

% Create the 'bad_name_files' directory if it doesn't exist
bad_name_dir = fullfile(file_dir, 'bad_name_files');
if ~exist(bad_name_dir, 'dir')
    fprintf('"bad_name_files" directory does not exist in %s. Creating the folder.\n', file_dir)
    mkdir(bad_name_dir);
end


% Ask for confirmation

response = input(sprintf('Copy %d .mat files? [y/n]: ', n), 's');

for i = 1:num_skipped_shots
    bad_files = bad_name_files{i};

    if ~strcmpi(response, 'y')
        disp('Aborted. No files copied.')
        return
    else
        n_bad_files = length(bad_files);
        fprintf('Copying %d files, from %s to %s.\n', n_bad_files, bad_files(1), bad_files(end))
        w = waitbar(0, sprintf('Copying 0/%d files', n_bad_files));
        for b = 1:n_bad_files
            waitbar(b/num_skipped_shots, w, sprintf('Copying %d/%d files', b, num_skipped_shots))
            % fprintf('Copy to %s\n',bad_name_dir)
            copyfile(fullfile(file_dir,bad_files(b)),bad_name_dir)

            fprintf('Delete %s\n', fullfile(file_dir,bad_files(b)))
            delete(fullfile(file_dir,bad_files(b)))
        end
        close(w)
    end
end

