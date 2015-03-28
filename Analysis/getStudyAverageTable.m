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
average_data_idx = 5;

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
            joints_average_over_time = scen_table(scen_table.Person_Id==p_id,:);

            average_row.Kinect_Config = k;
            average_row.Scenario_Id = scen_id;
            average_row.Person_Id = p_id;
            
            joints_average_over_time = joints_average_over_time(:,average_data_idx:average_data_idx+7);
            
            avg_all_dx = mean(joints_average_over_time{:,1});
            std_all_dx = std(joints_average_over_time{:,1});
            avg_all_dy = mean(joints_average_over_time{:,3});
            std_all_dy = std(joints_average_over_time{:,3});
            avg_all_dz = mean(joints_average_over_time{:,5});
            std_all_dz = std(joints_average_over_time{:,5});
            avg_all_dd = mean(joints_average_over_time{:,7});
            std_all_dd = std(joints_average_over_time{:,7});
            
            average_row.(joints_average_types{1,1}) = avg_all_dx;
            average_row.(joints_average_types{1,2}) = std_all_dx;
            average_row.(joints_average_types{1,3}) = avg_all_dy;
            average_row.(joints_average_types{1,4}) = std_all_dy;
            average_row.(joints_average_types{1,5}) = avg_all_dz;
            average_row.(joints_average_types{1,6}) = std_all_dz;
            average_row.(joints_average_types{1,7}) = avg_all_dd;
            average_row.(joints_average_types{1,8}) = std_all_dd;

            display(average_row);
            
            study_average_table(row_counter,:) = struct2table(average_row);
            row_counter = row_counter+1;

        end
    end
end

end
