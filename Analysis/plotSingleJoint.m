function [] = plotSingleJoint(joint_name, single_joint_table)
%UNTITLED Summary of this function goes here
%   Detailed explanation goes here
consts;

person_id = single_joint_table(1,single_c_person);
plot_title = sprintf('Person %d Joint %s',person_id,joint_name);
plot_filename = sprintf('Plots/Person_%d_Joint_%s',person_id,joint_name);

x = single_joint_table(:,single_c_timestamp);
dist_dx = single_joint_table(:,single_c_dx);
dx_sd = single_joint_table(:,single_c_dx_sd);
dist_dy = single_joint_table(:,single_c_dy);
dy_sd = single_joint_table(:,single_c_dy_sd);
dist_dz = single_joint_table(:,single_c_dz);
dz_sd = single_joint_table(:,single_c_dz_sd);
dist_dd = single_joint_table(:,single_c_dd);
dd_sd = single_joint_table(:,single_c_dd_sd);

figure;
hold on;
x_h = shadedErrorBar(x,dist_dx,dx_sd,'-r', 1);
y_h = shadedErrorBar(x,dist_dy,dy_sd,'-g', 1);
z_h = shadedErrorBar(x,dist_dz,dz_sd,'-b', 1);
d_h = shadedErrorBar(x,dist_dd,dd_sd,'-k', 1);
set(gcf,'visible','off')
hold off;

title(plot_title);
xlabel('Time(s)');
ylabel('Distance(cm)');
legend([x_h.mainLine,y_h.mainLine,z_h.mainLine,d_h.mainLine],'\Delta x','\Delta y','\Delta z','\Delta d','Location','northeastoutside');

set(gcf, 'PaperPositionMode', 'manual');
set(gcf, 'PaperUnits', 'normalized');
set(gcf, 'PaperPosition', [0 0 1 0.7])
print('-dsvg', '-painters', plot_filename);

end
