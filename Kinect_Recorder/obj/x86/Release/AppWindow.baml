   M S B A M L   `   `   `  ���� J  FKinect_Recorder, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:Kinect_RecorderKinect_Recorder      Kinect_Recorder.AppWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     /localclr-namespace:Kinect_Recorder     -     � Title$  Kinect Recorder�� � WindowStartupLocation$ CenterScreen=� � WindowState$ 	Maximized=�$��441.333��$��829.418��.��� .+��� -    ��Name$ ui_tabs��$��Stretch=�$��0,0,0,0q�$��Stretch=�$��20(�.@��� -    ��Name$ general_tabItem��$��General��.��� ���#1a1a1a$��0q�.+��� -    ��Name$ View_Feed_Canvas��$
��Left=�$	��148��$��	10,10,0,0q�$	��Top=�$	��388�����	#FF4C4B4B.+��� $��Kinect�� �� Left$ 75��
 �� Top$ 11��
��������� $��60��$ 10��$��60�� J� OpacityMask
 �   �$��Resources/kinect.png��$
��Fill=�$ 3���� -   	 ��Name$#	 kinect_connection_state_image��$��60��$ 10��$ 63��$��60��
 �   �$��Resources/red_circle.png��$
��Fill=��� -   
 ��Name$#
 kinect_connection_state_label��$��Disconnected��$ 75��$ 77��$��16(�
��������� -    ��Name$ kinect_view_feed_button��$��	View Feed��$	 214��$ 68��$	��134��$��46���� -   $ Recording_Canvas��$
��Left=�$��83��$	��Top=�$	��255�����	#FF4C4B4B$y�Hidden=�$	 181��$	 108��$��	10,10,0,0q�.+��� -	   $	 Recording_Image��$��60��$	 171��$ 10��$��60��
 �   �$ ��Resources/green_circle.png��$
��Fill=�d� -
    d�Name$ Record_Button��$��Record��$ 17��$	��134��$��46��$ 10��� -    �Name$ Kinect_Feed��$��Center=�$��Center=�$��0,60,0,0q�$y�Hidden=�.+�(�  (� Stretch$ Uniform=�$��Center=�$	��Top=�$��0q�.���� -   $��
depthImage��$	��812��$	��480��(� $ Uniform=�$��Center=�$��Center=�$��0q�.���� -   $��skeletonImage��$	��812��$	��480���� $
��Logs��.��� ���#1a1a1a.+��� -    ��Name$ logs_richtextbox��$��Stretch=�$��0,0,0,0q�$��Stretch=� �� 
IsReadOnly . $��16(����#1a1a1a
�������. �/� .T�J� .*�.*��� $� ��