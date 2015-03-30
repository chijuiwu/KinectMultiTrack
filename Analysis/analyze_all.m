dir_experiments_1_2_3_6 = '../Data/Experiments_1_2_3_6/';
dir_experiments_4 = '../Data/Experiments_4/';

% raw_data_table = table();
% 
% % loading data
% raw_data_table = readData(dir_experiments_1_2_3_6, raw_data_table);
% raw_data_table = readData(dir_experiments_4, raw_data_table);
% fprintf('All files loaded!!!\n');

disp('Cleaning data...');
tic;
data_table = cleanData(raw_data_table);
time = toc;
fprintf('Done!!!, time=%.2f\n',time);
% 
% disp('Creating difference table...');
% tic;
% [difference_table, difference_joint_types] = getJointDifferenceTable(data_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% disp('Creating time average table...');
% tic;
% [time_average_table, time_average_joint_types] = getTimeAverageTable(difference_table, difference_joint_types);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% disp('Creating joints average table...');
% tic;
% joints_average_table = getJointsAverageTable(time_average_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% disp('Creating study average table...');
% tic;
% study_average_table = getStudyAverageTable(joints_average_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% disp('Creating study joints table...');
% tic;
% study_joints_table = getStudyJointsTable(time_average_table, time_average_joint_types);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
