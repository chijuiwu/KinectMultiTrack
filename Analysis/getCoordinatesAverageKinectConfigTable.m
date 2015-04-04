function [coordinates_average_kinect_config_table] = getCoordinatesAverageKinectConfigTable(coordinates_average_study_table, coordinates_average_types)
%
% Get coordinates averages for each kinect config for the first three studies
% 
% Kinect_Config Person_Id ...
% Joints_avg_dx Joints_sd_dx Joints_avg_dy Joints_sd_dy ...
% Joints_avg_dz Joints_sd_dz Joints_avg_dd Joints_sd_dd
% 

joints_util;

first_variable_names = {
    'Kinect_Config','Person_Id'
};

% get count for people in all kinect configurations and scenarios
count_person_in_all_k = 0;
for k = unique(coordinates_average_study_table.Kinect_Config,'rows').'
    k_table = coordinates_average_study_table(coordinates_average_study_table.Kinect_Config==k,{'Scenario_Id','Person_Id'});
    scen_table = k_table(k_table.Scenario_Id==1 | k_table.Scenario_Id==2 | k_table.Scenario_Id==3,:);
    count_person_in_all_k = count_person_in_all_k + length(unique(scen_table.Person_Id,'rows').');
end

table_variable_names = [first_variable_names coordinates_average_types];
row_count = count_person_in_all_k + 1; % plus 1 for average
col_count = length(table_variable_names);
coordinates_average_kinect_config_table = array2table(zeros(row_count,col_count),'VariableNames',table_variable_names);
average_row = struct();
for field = table_variable_names
    average_row.(char(field)) = 0;
end

first_joint_idx = length(first_variable_names)+1;
dx=first_joint_idx;
dy=dx+2;
dz=dy+2;
dd=dz+2;

row_counter = 1;
for k = unique(coordinates_average_study_table.Kinect_Config,'rows').'
    k_table = coordinates_average_study_table(coordinates_average_study_table.Kinect_Config==k,:);
    
    fprintf('Calculating coordinate averages over kinect config - Kinect_Config=%d\n',k);

    first_three_scenarios = k_table.Scenario_Id==1 | ...
        k_table.Scenario_Id==2 | k_table.Scenario_Id==3;
    first_three_scenarios_table = k_table(first_three_scenarios,table_variable_names);
    
    
    %
    % first three
    %
    average_row.Kinect_Config = k;
    average_row.Person_Id = 0;
    
    c_avg_dx = mean(first_three_scenarios_table{:,dx});
    c_std_dx = std(first_three_scenarios_table{:,dx});
    c_avg_dy = mean(first_three_scenarios_table{:,dy});
    c_std_dy = std(first_three_scenarios_table{:,dy});
    c_avg_dz = mean(first_three_scenarios_table{:,dz});
    c_std_dz = std(first_three_scenarios_table{:,dz});
    c_avg_dd = mean(first_three_scenarios_table{:,dd});
    c_std_dd = std(first_three_scenarios_table{:,dd});

    c_avg_type_idx = 1;
    average_row.(coordinates_average_types{1,c_avg_type_idx}) = c_avg_dx;
    average_row.(coordinates_average_types{1,c_avg_type_idx+1}) = c_std_dx;
    average_row.(coordinates_average_types{1,c_avg_type_idx+2}) = c_avg_dy;
    average_row.(coordinates_average_types{1,c_avg_type_idx+3}) = c_std_dy;
    average_row.(coordinates_average_types{1,c_avg_type_idx+4}) = c_avg_dz;
    average_row.(coordinates_average_types{1,c_avg_type_idx+5}) = c_std_dz;
    average_row.(coordinates_average_types{1,c_avg_type_idx+6}) = c_avg_dd;
    average_row.(coordinates_average_types{1,c_avg_type_idx+7}) = c_std_dd;
    
    coordinates_average_kinect_config_table(row_counter,:) = struct2table(average_row);
    row_counter = row_counter+1;
    %
    % end first three
    %
end

% Average

fprintf('Calculating coordinates averages over kinect config - Average\n');

average_row.Kinect_Config = average_row.Kinect_Config+1;
average_row.Person_Id = 0; % hardcoded 1 person

c_avg_dx = mean(coordinates_average_kinect_config_table{:,dx});
c_std_dx = std(coordinates_average_kinect_config_table{:,dx});
c_avg_dy = mean(coordinates_average_kinect_config_table{:,dy});
c_std_dy = std(coordinates_average_kinect_config_table{:,dy});
c_avg_dz = mean(coordinates_average_kinect_config_table{:,dz});
c_std_dz = std(coordinates_average_kinect_config_table{:,dz});
c_avg_dd = mean(coordinates_average_kinect_config_table{:,dd});
c_std_dd = std(coordinates_average_kinect_config_table{:,dd});

c_avg_type_idx = 1;
average_row.(coordinates_average_types{1,c_avg_type_idx}) = c_avg_dx;
average_row.(coordinates_average_types{1,c_avg_type_idx+1}) = c_std_dx;
average_row.(coordinates_average_types{1,c_avg_type_idx+2}) = c_avg_dy;
average_row.(coordinates_average_types{1,c_avg_type_idx+3}) = c_std_dy;
average_row.(coordinates_average_types{1,c_avg_type_idx+4}) = c_avg_dz;
average_row.(coordinates_average_types{1,c_avg_type_idx+5}) = c_std_dz;
average_row.(coordinates_average_types{1,c_avg_type_idx+6}) = c_avg_dd;
average_row.(coordinates_average_types{1,c_avg_type_idx+7}) = c_std_dd;

coordinates_average_kinect_config_table(row_counter,:) = struct2table(average_row);
%


end
