joint_types = {
    'AnkleLeft','AnkleRight','ElbowLeft','ElbowRight', ...
    'FootLeft','FootRight','HandLeft','HandRight', ...
    'HandTipLeft','HandTipRight','Head','HipLeft','HipRight', ...
    'KneeLeft','KneeRight','Neck','ShoulderLeft','ShoulderRight', ...
    'SpineBase','SpineMid','SpineShoulder','ThumbLeft','ThumbRight'...
    'WristLeft','WristRight'
};

% 
% AnkleLeft_X, AnkleLeft_Y, AnkleLeft_Z, ...
% 
coordinate_types = {
    'X','Y','Z'
};
coordinate_joint_types = cell(1,length(joint_types)*length(coordinate_types));
counter = 0;
for jt = joint_types
    for c = coordinate_types
        counter = counter+1;
        coordinate_joint_types(1,counter) = strcat(jt,'_',c);
    end
end
