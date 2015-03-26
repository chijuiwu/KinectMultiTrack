dir_experiments_1_2_3_6 = '../Data/Experiments_1_2_3_6/';
dir_experiments_4 = '../Data/Experiments_4/';

% testing
filename = strcat(dir_experiments_1_2_3_6,'Study_0_Kinect_0_Time_11_35_09_AM.csv');
fprintf('Processing file, filename=%s\n',filename);
% valid = validateFile(filename);
% if ~valid
%     exit();
% end


raw_data_table = readtable(filename,'ReadVariableNames',true);
data_table = cleanData(raw_data_table);

tic;
[difference_table, difference_joint_types] = getJointDifferenceTable(data_table);
time = toc;
fprintf('Calculated difference table, time=%.2f\n',time);

tic;
time_average_table = getTimeAverageTable(difference_table, difference_joint_types);
time = toc;
fprintf('Calculated time average table, time=%.2f\n',time);

tic;
joints_average_table = getJointsAverageTable(time_average_table);
time = toc;
fprintf('Calculated joints average table, time=%.2f\n',time);

tic;
study_average_table = getStudyAverageTable(joints_average_table);
time = toc;
fprintf('Calculated study average table, time=%.2f\n',time);
