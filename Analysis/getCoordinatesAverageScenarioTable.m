function [coordinates_average_scenario_table] = getCoordinatesAverageScenarioTable(coordinates_average_study_table, coordinates_average_types)
%
% Get coordinates averages for each of the first three scenarios scenario for
% all kinect configurations
% 
% Scenario_Id Person_Id ...
% Joints_avg_dx Joints_sd_dx Joints_avg_dy Joints_sd_dy ...
% Joints_avg_dz Joints_sd_dz Joints_avg_dd Joints_sd_dd
% 

joints_util;

first_variable_names = {
    'Scenario_Id','Person_Id'
};

% get count for people in all kinect configurations and scenarios
count_person_in_all_scen = 0;
for scen_id = unique(coordinates_average_study_table.Scenario_Id,'rows').'
    
    if (scen_id ~= 1 || scen_id ~= 2 || scen_id ~= 3)
        continue;
    end
    
    scen_table = coordinates_average_study_table(coordinates_average_study_table.Scenario_Id==scen_id,{'Kinect_Config','Person_Id'});
    count_person_in_all_scen = count_person_in_all_scen + length(unique(scen_table.Person_Id,'rows').');
    
end

table_variable_names = [first_variable_names coordinates_average_types];
row_count = count_person_in_all_scen + 1; % plus 1 for average
col_count = length(table_variable_names);
coordinates_average_scenario_table = array2table(zeros(row_count,col_count),'VariableNames',table_variable_names);
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
for scen_id = unique(coordinates_average_study_table.Scenario_Id,'rows').'
    scen_table = coordinates_average_study_table(coordinates_average_study_table.Scenario_Id==scen_id,:);

    if (scen_id ~= 1 && scen_id ~= 2 && scen_id ~= 3)
        continue;
    end
    
    fprintf('Calculating coordinate averages over scenario - Scenario=%d\n',scen_id);
    
    for p_id = unique(scen_table.Person_Id,'rows').'
        person_in_scen = scen_table(scen_table.Person_Id==p_id,table_variable_names);
        
        average_row.Scenario_Id = scen_id;
        average_row.Person_Id = p_id;
        
        c_avg_dx = mean(person_in_scen{:,dx});
        c_std_dx = std(person_in_scen{:,dx});
        c_avg_dy = mean(person_in_scen{:,dy});
        c_std_dy = std(person_in_scen{:,dy});
        c_avg_dz = mean(person_in_scen{:,dz});
        c_std_dz = std(person_in_scen{:,dz});
        c_avg_dd = mean(person_in_scen{:,dd});
        c_std_dd = std(person_in_scen{:,dd});

        c_avg_type_idx = 1;
        average_row.(coordinates_average_types{1,c_avg_type_idx}) = c_avg_dx;
        average_row.(coordinates_average_types{1,c_avg_type_idx+1}) = c_std_dx;
        average_row.(coordinates_average_types{1,c_avg_type_idx+2}) = c_avg_dy;
        average_row.(coordinates_average_types{1,c_avg_type_idx+3}) = c_std_dy;
        average_row.(coordinates_average_types{1,c_avg_type_idx+4}) = c_avg_dz;
        average_row.(coordinates_average_types{1,c_avg_type_idx+5}) = c_std_dz;
        average_row.(coordinates_average_types{1,c_avg_type_idx+6}) = c_avg_dd;
        average_row.(coordinates_average_types{1,c_avg_type_idx+7}) = c_std_dd;
        
        coordinates_average_scenario_table(row_counter,:) = struct2table(average_row);
        row_counter = row_counter+1;
        
    end
end

% Average

fprintf('Calculating coordinates averages over scenarios - Average\n');

average_row.Scenario_Id = average_row.Scenario_Id+1;
average_row.Person_Id = 0; % hardcoded 1 person

c_avg_dx = mean(coordinates_average_scenario_table{:,dx});
c_std_dx = std(coordinates_average_scenario_table{:,dx});
c_avg_dy = mean(coordinates_average_scenario_table{:,dy});
c_std_dy = std(coordinates_average_scenario_table{:,dy});
c_avg_dz = mean(coordinates_average_scenario_table{:,dz});
c_std_dz = std(coordinates_average_scenario_table{:,dz});
c_avg_dd = mean(coordinates_average_scenario_table{:,dd});
c_std_dd = std(coordinates_average_scenario_table{:,dd});

c_avg_type_idx = 1;
average_row.(coordinates_average_types{1,c_avg_type_idx}) = c_avg_dx;
average_row.(coordinates_average_types{1,c_avg_type_idx+1}) = c_std_dx;
average_row.(coordinates_average_types{1,c_avg_type_idx+2}) = c_avg_dy;
average_row.(coordinates_average_types{1,c_avg_type_idx+3}) = c_std_dy;
average_row.(coordinates_average_types{1,c_avg_type_idx+4}) = c_avg_dz;
average_row.(coordinates_average_types{1,c_avg_type_idx+5}) = c_std_dz;
average_row.(coordinates_average_types{1,c_avg_type_idx+6}) = c_avg_dd;
average_row.(coordinates_average_types{1,c_avg_type_idx+7}) = c_std_dd;

coordinates_average_scenario_table(row_counter,:) = struct2table(average_row);

end
