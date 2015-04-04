function [coordinates_average_study_table, coordinates_average_types, coordinates_average_study_merged_table] = getCoordinatesAverageStudyTable(joints_average_study_table)
%
% Get joints averages for each study
% 
% Kinect_Config Scenario_Id Person_Id ...
% Joints_avg_dx Joints_sd_dx Joints_avg_dy Joints_sd_dy ...
% Joints_avg_dz Joints_sd_dz Joints_avg_dd Joints_sd_dd
% 

joints_util;

first_variable_names = {
    'Kinect_Config','Scenario_Id','Person_Id'
};

% 
% Joints_avg_dx Joints_sd_dx Joints_avg_dy Joints_sd_dy ...
% Joints_avg_dz Joints_sd_dz Joints_avg_dd Joints_sd_dd
% 
coordinates_average_types = {
    'Joints_avg_dx','Joints_std_dx','Joints_avg_dy','Joints_std_dy', ...
    'Joints_avg_dz','Joints_std_dz','Joints_avg_dd','Joints_std_dd'
};

table_variable_names = [first_variable_names coordinates_average_types];
row_count = size(joints_average_study_table,1);
col_count = length(table_variable_names);
coordinates_average_study_table = array2table(zeros(row_count,col_count),'VariableNames',table_variable_names);
average_row = struct();
for field = table_variable_names
    average_row.(char(field)) = 0;
end

first_joint_dx = length(first_variable_names)+1;
first_joint_dy = first_joint_dx + 2;
first_joint_dz = first_joint_dy + 2;
first_joint_dd = first_joint_dz + 2;
last_joint_dd = length(first_variable_names)+length(joint_types)*8;

row_counter = 1;
for k = unique(joints_average_study_table.Kinect_Config,'rows').'
    k_table = joints_average_study_table(joints_average_study_table.Kinect_Config==k,:);

    for scen_id = unique(k_table.Scenario_Id,'rows').'
        scen_table = k_table(k_table.Scenario_Id==scen_id,:);

        for p_id = unique(scen_table.Person_Id,'rows').'
            person_in_k_scen = scen_table(scen_table.Person_Id==p_id,:);

            fprintf('Calculating coordinate averages over study - Kinect_Config=%d, Scenario=%d, Person=%d\n', ...
                k, scen_id, p_id);
            
            average_row.Kinect_Config = k;
            average_row.Scenario_Id = scen_id;
            average_row.Person_Id = p_id;
            
             % 8 because Joint_avg_dx, Joint_sd_dx, ... Joint_std_dd
            person_all_dx = person_in_k_scen(:,first_joint_dx:8:last_joint_dd);
            person_all_dy = person_in_k_scen(:,first_joint_dy:8:last_joint_dd);
            person_all_dz = person_in_k_scen(:,first_joint_dz:8:last_joint_dd);
            person_all_dd = person_in_k_scen(:,first_joint_dd:8:last_joint_dd);

            avg_all_dx = mean(person_all_dx{:,:});
            std_all_dx = std(person_all_dx{:,:},0,2);
            avg_all_dy = mean(person_all_dy{:,:},2);
            std_all_dy = std(person_all_dy{:,:},0,2);
            avg_all_dz = mean(person_all_dz{:,:},2);
            std_all_dz = std(person_all_dz{:,:},0,2);
            avg_all_dd = mean(person_all_dd{:,:},2);
            std_all_dd = std(person_all_dd{:,:},0,2);

            average_row.(coordinates_average_types{1,1}) = avg_all_dx;
            average_row.(coordinates_average_types{1,2}) = std_all_dx;
            average_row.(coordinates_average_types{1,3}) = avg_all_dy;
            average_row.(coordinates_average_types{1,4}) = std_all_dy;
            average_row.(coordinates_average_types{1,5}) = avg_all_dz;
            average_row.(coordinates_average_types{1,6}) = std_all_dz;
            average_row.(coordinates_average_types{1,7}) = avg_all_dd;
            average_row.(coordinates_average_types{1,8}) = std_all_dd;
            
            coordinates_average_study_table(row_counter,:) = struct2table(average_row);
            row_counter = row_counter+1;

        end
    end
end

% 
% merge scenarios 4, 5, 8
% 

collapsed_table = coordinates_average_study_table;
[row,~] = find(coordinates_average_study_table.Scenario_Id == 5 | coordinates_average_study_table.Scenario_Id == 8);
for r = row.'
    collapsed_table{r,'Scenario_Id'} = 4;
end

% hardcded 
row_count = row_count-5;
coordinates_average_study_merged_table = array2table(zeros(row_count,col_count),'VariableNames',table_variable_names);

row_counter = 1;
for k = unique(collapsed_table.Kinect_Config,'rows').'
    k_table = collapsed_table(collapsed_table.Kinect_Config==k,:);

    for scen_id = unique(k_table.Scenario_Id,'rows').'
        scen_table = k_table(k_table.Scenario_Id==scen_id,:);
        
        if (scen_id == 4)
            average_row.(table_variable_names{1}) = k;
            average_row.(table_variable_names{2}) = scen_id;
            % hardcoded - also collapsed to one person
            average_row.(table_variable_names{3}) = 0;
            
            average_row.(table_variable_names{4}) = mean(scen_table{:,4});
            average_row.(table_variable_names{5}) = std(scen_table{:,4});
            average_row.(table_variable_names{6}) = mean(scen_table{:,6});
            average_row.(table_variable_names{7}) = std(scen_table{:,6});
            average_row.(table_variable_names{8}) = mean(scen_table{:,8});
            average_row.(table_variable_names{9}) = std(scen_table{:,8});
            average_row.(table_variable_names{10}) = mean(scen_table{:,10});
            average_row.(table_variable_names{11}) = std(scen_table{:,10});
            
            coordinates_average_study_merged_table(row_counter,:) = struct2table(average_row);
        else
            rows = collapsed_table.Kinect_Config==k & collapsed_table.Scenario_Id==scen_id;
            coordinates_average_study_merged_table(row_counter,:) = collapsed_table(rows,:);
        end
        
        row_counter = row_counter + 1;
        
    end
end

end

