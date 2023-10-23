/// <summary>
/// Json映射的实体类 以下代码自动生成
/// 接口地址：
/// 创建时间：2023/10/23 11:03:23
/// </summary>
public class Feature_People_Perception
{  
	public string robotId;
	public int timestamp;
	public Feature_People_Perception_data data;
	public object clientId;
	public class Feature_People_Perception_data
	{
		public Feature_People_Perception_data_feature[] feature;
		public string featureId;
	}
	public class Feature_People_Perception_data_feature
	{
		public int gender;
		public float[] bbox;
		public int visitor_id;
		public Feature_People_Perception_data_feature_person_point person_point;
		public float[] location_world;
		public int loss_reason;
		public int detect_id;
		//public object[] person_2d_keypoint;
		public float angle;
		public int speak;
		public int mask;
		public int glass;
		public int[] body_pose;
		//public object[] person_3d_keypoint;
		public float[] velocity;
		public Feature_People_Perception_data_feature_person_action person_action;
		public int[] camera_location;
		public int mouth;
		public Feature_People_Perception_data_feature_gaze gaze;
		public int track_id;
		public string move_dir_x;
		public int location_confidence;
		public int loss_track_time;
		public string move_dir_y;
		public float[] location;
		public Feature_People_Perception_data_feature_time time;
		public int[] face_pose_world;
		public Feature_People_Perception_data_feature_intention_info intention_info;
		public int age;
		public int status;
	}



    public class Feature_People_Perception_data_feature_person_point
	{
		public int left_judge;
		public int right_judge;
		public float[] left_dir;
		public float[] right_dir;
		public float[] left_pos;
		public float[] right_pos;
	}
	public class Feature_People_Perception_data_feature_person_action
	{
		public object hand_shake;
		public object take_a_photo;
		public object point_to_an_object;
		public object read;
		public object talk_to_a_person;
		public object touch_an_object;
		public object grab_a_person;
		public object hand_wave;
		public object text_on_look_at_a_cellphone;
		public object watch_TV;
		public object watch_a_person;
		public object drink;
		public object unknown;
		public object hug_a_person;
		public object hand_clap;
		public object turn_a_screwdriver;
		public object listen_to_a_person;
		public object answer_phone;
		public object stand;
		public object walk;
		public object sit;
	}
	public class Feature_People_Perception_data_feature_gaze
	{
		public float conf;
		public object location;
		public string target;
	}
	public class Feature_People_Perception_data_feature_time
	{
		public int nsecs;
		public int secs;
	}
	public class Feature_People_Perception_data_feature_intention_info
	{
		public int body_left_refer;
		public bool engagement_with_location;
		public bool engagement;
		public int body_right_refer;
		public int head_refer;
	}
}