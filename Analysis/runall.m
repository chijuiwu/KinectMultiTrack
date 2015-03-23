dir_experiments_1_2_3_6 = '../Data/Experiments_1_2_3_6/';
dir_experiments_4 = '../Data/Experiments_4/';

% testing
filename = strcat(dir_experiments_4, 'Study_0_Kinect_0_Time_11_35_09_AM.csv';

valid = validateFile(filename);
if ~valid
    exit();
end

analysis(filename);