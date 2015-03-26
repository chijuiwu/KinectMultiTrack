function [study_average_table] = getStudyAverageTable(joints_average_table)
%
% Kinect_Config Scenario_Id Person_Id ...
% Joints_avg_dx Joints_sd_dx Joints_avg_dy Joints_sd_dy ...
% Joints_avg_dz Joints_sd_dz Joints_avg_dd Joints_sd_dd
% 

joints_util;

first_variable_names = {
    'Kinect_Config','Scenario_Id','Person_Id'
};
first_average_idx = length(first_variable_names)+1;

% 
% Joints_avg_dx Joints_sd_dx Joints_avg_dy Joints_sd_dy ...
% Joints_avg_dz Joints_sd_dz Joints_avg_dd Joints_sd_dd
% 
joints_average_types = {
    'Joints_avg_dx','Joints_sd_dx','Joints_avg_dy','Joints_sd_dy', ...
    'Joints_avg_dz','Joints_sd_dz','Joints_avg_dd','Joints_sd_dd'
};

% get row count
total_row_count = length(unique(joints_average_table.Study_Id,'rows').');

table_variable_names = [first_variable_names joints_average_types];
row_count = total_row_count;
col_count = length(table_variable_names);
study_average_table = array2table(zeros(row_count,col_count),'VariableNames',table_variable_names);
average_row = struct();
for field = table_variable_names
    average_row.(char(field)) = 0;
end

row_counter = 1;
for k = unique(joints_average_table.Kinect_Config,'rows').'
    k_table = joints_average_table(joints_average_table.Kinect_Config==k,:);

    for scen_id = unique(k_table.Scenario_Id,'rows').'
        scen_table = k_table(k_table.Scenario_Id==scen_id,:);

        for p_id = unique(scen_table.Person_Id,'rows').'
            person_joints_data = scen_table(scen_table.Person_Id==p_id,:);

            average_row.Kinect_Config = k;
            average_row.Scenario_Id = scen_id;
            average_row.Person_Id = p_id;
            
            % offset 1 because previous table (time_average_table contains Study_Id)
            average_data_idx = first_average_idx+1;
            average_data_over_time = person_joints_data(:,average_data_idx:average_data_idx+7);
            
            % 4 difference avg types (dx, dy, dz, dd)
            for avg_type_num = 1:4
                avg_idx = 1+(avg_type_num-1)*2;
                all_study_avg = mean(average_data_over_time{:,avg_idx},2);
                all_study_std = std(average_data_over_time{:,avg_idx},2);
                average_row.(joints_average_types{1,avg_idx}) = all_study_avg;
                average_row.(joints_average_types{1,avg_idx+1}) = all_study_std;
            end
            
            study_average_table(row_counter,:) = struct2table(average_row);
            row_counter = row_counter+1;

        end
    end
end

end
