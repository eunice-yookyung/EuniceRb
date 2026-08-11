function rename_skipped_shots(fileDir_renamed, fileDir_original)

if nargin < 1
    fileDir_renamed = '7_EandU_1067_1078_wall2500_8p5_bump2p26 - Copy';
end

if nargin < 2
    fileDir_original = [fileDir_renamed '\bad_name_files'];
end

current_batch_size = 62;    % number of measurements per batch that 
                            % has been set in the Andor software by mistake
wanted_batch_size = 62;     % actual number of measurements per batch

% Want to rename all three types of files -> loop 
fileendings = {'.fits', 'atomMatrix.mat', '.mat'}; 
for j = 1:3
    % ALWAYS ADJUST THESE ACCORDING TO YOUR NEEDS 
    beginning = strcat(fileDir_original,'\scanAA026',fileendings{j});  %beginning of falsely numbered files
    ending = strcat(fileDir_original,'\scanAD057',fileendings{j});     %end of falsely numbered files
    new_beginning = strcat(fileDir_renamed,'\scanAA027',fileendings{j}); %first name of renamed files

    % Create array of existing file names that are to be renamed (checked_list)
    listing = dir([fileDir_original '\*' fileendings{j}]);
    if j == 3 % If you are only looking for files of the form 'XX###.mat', you need to manually exclude the extra ones named 'XX###atomMatrix.mat'
        listing = listing(~contains({listing.name}, 'atomMatrix.mat'));
        size(listing)
    end
    checked_list = cell(1,length(listing));
    for i = 1:length(listing)
        filename = fullfile(fileDir_original, listing(i).name); 
        checked_list{i} = filename;
    end
    indbeginning = find(ismember(checked_list,beginning)); 
    indending = find(ismember(checked_list,ending));
    numfiles = (indending-indbeginning)+1;
    checked_list = checked_list(indbeginning:indending); 

    % Create array of new file names (new_checked_list) 
    alphabet = {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};
    new_checked_list = {};
    count = 1;  
    for i = 1:26
        for ii = 1:26
            for iii = 1:wanted_batch_size
                new_checked_list{count} = strcat(fileDir_renamed,'\scan',alphabet{i},alphabet{ii},sprintf('%03.0f',iii),fileendings{j});
                count = count+1; 
            end 
        end 
    end
    new_indbeginning = find(ismember(new_checked_list,new_beginning)); 
    new_indending = new_indbeginning+numfiles-1; 
    new_checked_list = new_checked_list(new_indbeginning:new_indending); 

    for i = 1:numfiles
       copyfile(checked_list{i},new_checked_list{i});  
    end 
end 
