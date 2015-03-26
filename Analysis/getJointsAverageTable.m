function [joints_average_table] = getJointsAverageTable(time_average_table)
%
% Study_Id Kinect_Config Scenario_Id Person_Id ...
% Joints_avg_dx Joints_sd_dx Joints_avg_dy Joints_sd_dy ...
% Joints_avg_dz Joints_sd_dz Joints_avg_dd Joints_sd_dd
% 

joints_util;

first_variable_names = {
    'Study_Id','Kinect_Config','Scenario_Id','Person_Id'
};
first_joint_idx = length(first_variable_names)+1;

% 
% Joints_avg_dx Joints_sd_dx Joints_avg_dy Joints_sd_dy ...
% Joints_avg_dz Joints_sd_dz Joints_avg_dd Joints_sd_dd
% 
joints_average_types = {
    'Joints_avg_dx','Joints_sd_dx','Joints_avg_dy','Joints_sd_dy', ...
    'Joints_avg_dz','Joints_sd_dz','Joints_avg_dd','Joints_sd_dd'
};

table_variable_names = [first_variable_names joints_average_types];
row_count = size(time_average_table,1);
col_count = length(table_variable_names);
joints_average_table = array2table(zeros(row_count,col_count),'VariableNames',table_variable_names);
average_row = struct();
for field = table_variable_names
    average_row.(char(field)) = 0;
end

% first_joint_idx same in time_average_table
first_dx = first_joint_idx;
first_dy = first_joint_idx+1;
first_dz = first_joint_idx+2;
first_dd = first_joint_idx+3;
last_dd = length(joint_types)*8;

row_counter = 1;
for s_id = unique(time_average_table.Study_Id,'rows').'
    s_table = time_average_table(time_average_table.Study_Id==s_id,:);
    
    for k = unique(s_table.Kinect_Config,'rows').'
        k_table = s_table(s_table.Kinect_Config==k,:);
        
        for scen_id = unique(k_table.Scenario_Id,'rows').'
            scen_table = k_table(k_table.Scenario_Id==scen_id,:);
            
            for p_id = unique(scen_table.Person_Id,'rows').'
                person_joints_data = scen_table(scen_table.Person_Id==p_id,:);
                
                average_row.Study_Id = s_id;
                average_row.Kinect_Config = k;
                average_row.Scenario_Id = scen_id;
                average_row.Person_Id = p_id;
          
                % 8 because Joint_avg_dx, Joint_sd_dx, ...
                all_dx = person_joints_data(:,first_dx:8:last_dd);
                all_dy = person_joints_data(:,first_dy:8:last_dd);
                all_dz = person_joints_data(:,first_dz:8:last_dd);
                all_dd = person_joints_data(:,first_dd:8:last_dd);
                
                avg_all_dx = mean(all_dx{:,:},2);
                std_all_dx = std(all_dx{:,:},0,2);
                avg_all_dy = mean(all_dy{:,:},2);
                std_all_dy = std(all_dy{:,:},0,2);
                avg_all_dz = mean(all_dz{:,:},2);
                std_all_dz = std(all_dz{:,:},0,2);
                avg_all_dd = mean(all_dd{:,:},2);
                std_all_dd = std(all_dd{:,:},0,2);

                average_row.(joints_average_types{1,1}) = avg_all_dx;
                average_row.(joints_average_types{1,2}) = std_all_dx;
                average_row.(joints_average_types{1,3}) = avg_all_dy;
                average_row.(joints_average_types{1,4}) = std_all_dy;
                average_row.(joints_average_types{1,5}) = avg_all_dz;
                average_row.(joints_average_types{1,6}) = std_all_dz;
                average_row.(joints_average_types{1,7}) = avg_all_dd;
                average_row.(joints_average_types{1,8}) = std_all_dd;

                joints_average_table(row_counter,:) = struct2table(average_row);
                row_counter = row_counter+1;
        
            end
        end
    end
end

end
