% function bad_name_files = time_lapse_check(fileDir)

% if nargin < 1
    fileDir = '5_EandU_106107_107108_walls_2p249_bump1p9948_time44p9ms';
% end

prefix = 'scan';
listing = dir(fullfile(fileDir, [prefix '*.fits']));
n_listing = length(listing);
dates = datetime({listing.date}');
time_diff_list = [0; seconds(diff(dates))]; % the zero is prepended to be consistent with the previous definition of time_diff_list.

wrong_index = find(time_diff_list > 45); % set the time difference to look for in s
disp('Possible skipped shots around: ')
if ~isempty(wrong_index) 
    
    % for each of the bad file name ranges, collect the names
    bad_name_files = cell(length(wrong_index));

    for i = 1:length(wrong_index)

        num_files = n_listing - (wrong_index(i)-1) + 1;

        fprintf('[Range %d] %s.\nTime Lapse: %d seconds. \n%s to %s should go in bad_name_files (%d files).\n', ...
            i,...
            listing(wrong_index(i)).name, ...
            time_diff_list(wrong_index(i)), ...
            listing(wrong_index(i)-1).name, ...
            listing(end).name, ...
            num_files)
    
        
        bad_name_files{i} = string({listing(wrong_index(i)-1:num_files).name})';
    end
    
else 
    disp('None')
end 