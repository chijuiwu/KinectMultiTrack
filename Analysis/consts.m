% Headers
col_timestamp = 1;
col_person = 2;
col_skeleton = 3;
col_fov = 4;
col_skeleton_timestamp = 5;
col_headers = col_timestamp:col_skeleton_timestamp;

% Joints
col_joint = 6;
col_x = col_joint;
col_y = col_joint + 1;
col_z = col_joint + 2;
total_joints = 25;

% Count
headers_count = total_joints*3+length(col_headers);