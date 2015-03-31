function [coordinates_average_kinect_config_table] = getCoordinatesAverageKinectConfigTable(coordinates_average_study_table, coordinates_average_types)
%
% Get joints averages for each study
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
    for scen_id = unique(k_table.Scenario_Id,'rows').'
        if (scen_id == 1)
            scen_table = k_table(k_table.Scenario_Id==scen_id,:);
            count_person_in_all_k = count_person_in_all_k + length(unique(scen_table.Person_Id,'rows').');
        end
    end
end

table_variable_names = [first_variable_names coordinates_average_types];
row_count = count_person_in_all_k;
col_count = length(table_variable_names);
coordinates_average_kinect_config_table = array2table(zeros(row_count,col_count),'VariableNames',table_variable_names);
average_row = struct();
for field = table_variable_names
    average_row.(char(field)) = 0;
end

avg_dx_idx = 4;

row_counter = 1;
for k = unique(coordinates_average_study_table.Kinect_Config,'rows').'
    k_table = coordinates_average_study_table(coordinates_average_study_table.Kinect_Config==k,:);
    
    fprintf('Calculating coordinate averages over kinect config - Kinect_Config=%d\n',k);

    first_three_scenarios = k_table.Scenario_Id==1 | ...
        k_table.Scenario_Id==2 | k_table.Scenario_Id==3;
    first_three_scenarios_table = k_table(first_three_scenarios,:);
    
    %
    % first three
    %
    average_row.Kinect_Config = k;
    average_row.Person_Id = 0;
    
    c_avg_dx = mean(first_three_scenarios_table{:,avg_dx_idx});
    c_std_dx = std(first_three_scenarios_table{:,avg_dx_idx});
    c_avg_dy = mean(first_three_scenarios_table{:,avg_dx_idx+2});
    c_std_dy = std(first_three_scenarios_table{:,avg_dx_idx+2});
    c_avg_dz = mean(first_three_scenarios_table{:,avg_dx_idx+4});
    c_std_dz = std(first_three_scenarios_table{:,avg_dx_idx+4});
    c_avg_dd = mean(first_three_scenarios_table{:,avg_dx_idx+6});
    c_std_dd = std(first_three_scenarios_table{:,avg_dx_idx+6});

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

end
