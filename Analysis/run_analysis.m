dir_experiments_1_2_3_6 = '../Data/Experiments_1_2_3_6/';
dir_experiments_4 = '../Data/Experiments_4/';

% testing
filename = strcat(dir_experiments_1_2_3_6,'Study_0_Kinect_0_Time_11_35_09_AM.csv');

display(filename);

% valid = validateFile(filename);
% if ~valid
%     exit();
% end

% analysis(filename);

table = readtable(filename,'ReadVariableNames',true);
% display(table);

row_study_id = table{:,{'Study'}};
display(row_study_id);

difference_table = getDifferenceTable(table);