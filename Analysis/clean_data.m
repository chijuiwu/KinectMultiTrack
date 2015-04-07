function [clean_data_table] = clean_data(data_table)
% 
% 
% 

joints_util;

clean_data_table = data_table;

% 
% Changing wrong scenario ids 4 to 8
% 
fprintf('Fixing wrong scenario id due to code...\n');
tic;
[row,~] = find(clean_data_table.Scenario_Id == 4);
for r = row.'
    if clean_data_table{r-1,'Scenario_Id'}(1) == 8
        clean_data_table{r,'Scenario_Id'} = 8;
    end
end
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

% 
% Set time intervals
%
fprintf('Setting time intervals...\n');
tic;
for s_id = unique(data_table.Study_Id,'rows').'
    s_table = data_table(data_table.Study_Id==s_id,{'Kinect_Config','Scenario_Id','Tracker_Time'});
    
    for k = unique(s_table.Kinect_Config,'rows').'
        k_table = s_table(s_table.Kinect_Config==k,{'Scenario_Id','Tracker_Time'});
        
        for scen_id = unique(k_table.Scenario_Id,'rows').'
            scen_table = k_table(k_table.Scenario_Id==scen_id,{'Tracker_Time'});
  
            fprintf('Setting time interval - Participant: %d, Kinect_Config: %d, Scenario_Id: %d\n', s_id, k, scen_id);
            
            init_time = scen_table{1,1};
            
            rows = clean_data_table.Study_Id==s_id & ...
                clean_data_table.Kinect_Config==k & ...
                clean_data_table.Scenario_Id==scen_id;
            clean_data_table{rows,'Tracker_Time'} = clean_data_table{rows,'Tracker_Time'}-init_time;
        end
    end
end
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

% 
% Convert time millisec to sec
% 
fprintf('Converting tracker time to seconds...\n');
tic;
clean_data_table{:,'Tracker_Time'} = clean_data_table{:,'Tracker_Time'}/1000;
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

% 
% Convert coordinate meters to centimeters
% 
fprintf('Converting coordinates to centimeters...\n');
tic;
clean_data_table{:,coordinate_joint_types} = clean_data_table{:,coordinate_joint_types}*100;
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

% 
% Keep only duplicate skeletons
% 
fprintf('Removing single skeletons...\n');
tic;
for s_id = unique(clean_data_table.Study_Id,'rows').'
    s_table = clean_data_table(clean_data_table.Study_Id==s_id, ...
        {'Study_Id','Kinect_Config','Scenario_Id','Tracker_Time','Person_Id','Skeleton_Id'});
    
    for k = unique(s_table.Kinect_Config,'rows').'
        k_table = s_table(s_table.Kinect_Config==k, ...
            {'Kinect_Config','Scenario_Id','Tracker_Time','Person_Id','Skeleton_Id'});
        
        for scen_id = unique(k_table.Scenario_Id,'rows').'
            scen_table = k_table(k_table.Scenario_Id==scen_id, ...
                {'Scenario_Id','Tracker_Time','Person_Id','Skeleton_Id'});
            
            fprintf('Checking single skeletons - Participant: %d, Kinect_Config: %d, Scenario_Id: %d\n', s_id, k, scen_id);
            
            for t = unique(scen_table.Tracker_Time,'rows').'
                t_table = scen_table(scen_table.Tracker_Time==t, ...
                    {'Tracker_Time','Person_Id','Skeleton_Id'});
                
                for p_id = unique(t_table.Person_Id,'rows').'
                    p_table = t_table(t_table.Person_Id==p_id, ...
                        {'Person_Id','Skeleton_Id'});
                    
                    skel_row_count = size(p_table,1);
                    if (skel_row_count < 2)
                        rows = ...
                            clean_data_table.Study_Id==s_id & ...
                            clean_data_table.Kinect_Config==k & ...
                            clean_data_table.Scenario_Id==scen_id & ...
                            clean_data_table.Tracker_Time==t & ...
                            clean_data_table.Person_Id==p_id;
                        clean_data_table(rows,:) = [];
                    end
                end
            end
        end
    end
end
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

end

