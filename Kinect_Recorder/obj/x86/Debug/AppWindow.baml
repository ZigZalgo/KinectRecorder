   M S B A M L   `   `   `  ����J  FKinect_Recorder, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:Kinect_RecorderKinect_Recorder  5   	       Kinect_Recorder.AppWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   /localclr-namespace:Kinect_Recorder     5   	   -   5          � Title$  Kinect Recorder��5   	    � WindowStartupLocation$ CenterScreen=�5	   	    � WindowState$ 	Maximized=�5
   	   $��441.333��5   	   $��829.418��6   .��5      � .+�5   
   �� -   5         ��Name$ ui_tabs��5      $��Stretch=�6&   $��0,0,0,0q�6D   $��Stretch=�6U   $��20(�6q   .@�5      �� -   5         ��Name$ general_tabItem��5      $��General��6/   .��5      � ���#1a1a1a$��0q�6,   .+�5      �� -   5         ��Name$ View_Feed_Canvas��5      $
��Left=�67   $	��148��6R   $��	10,10,0,0q�6_   $	��Top=�6r   $	��388��6�   ���	#FF4C4B4B.+�5      �� $��Kinect��6     �� Left$ 75��61   
 �� Top$ 11��6B   
�������6R   �� 5      $��60��6    $ 10��6,   $��60��6=    J� OpacityMask
 �   �$��Resources/kinect.png��6\   $
��Fill=�6z   $ 3��6�   �� 5      -   5        	 ��Name$#	 kinect_connection_state_image��5       $��60��6G   $ 10��6S   $ 63��6d   $��60��6t   
 �   �$��Resources/red_circle.png��6�   $
��Fill=�6�   �� 5      -   5        
 ��Name$#
 kinect_connection_state_label��5       $��Disconnected��6G   $ 75��6^   $ 77��6o   $��16(�6   
�������6�   �� 5      -   5         ��Name$ kinect_view_feed_button��5   !   $��	View Feed��6B   $	 214��6}   $ 68��6�   $	��134��6�   $��46��6�   5      �� 5!      -   5        $ Recording_Canvas��5!      $
��Left=�67   $��83��6R   $	��Top=�6^   $	��255��6v   ���	#FF4C4B4B$y�Hidden=�6�   $	 181��6�   $	 108��6�   $��	10,10,0,0q�6�   .+�5"      �� -	   5        $	 Recording_Image��5"       $��60��69   $	 171��6E   $ 10��6W   $��60��6g   
 �   �$ ��Resources/green_circle.png��6�   $
��Fill=�6�   d� 5#      -
   5         d�Name$ Record_Button��5#   '   $��Record��6>   $ 17��6f   $	��134��6v   $��46��6�   $ 10��6�   5%      � 5(      -   5         �Name$ Kinect_Feed��5(      $��Center=�60   $��Center=�6M   $��0,60,0,0q�6h   $y�Hidden=�6z   .+�5)      (�  (� Stretch$ Uniform=�6"   $��Center=�64   $	��Top=�6Q   $��0q�6i   .��5*      �� -   5        $��
depthImage��5*   $   $	��812��66   $	��480��6B   5+      (� 5,      $ Uniform=�6"   $��Center=�64   $��Center=�6Q   $��0q�6l   .��5-      �� -   5        $��skeletonImage��5-   $   $	��812��69   $	��480��6E   5.      5/      53      54      �� 58      $
��Logs��6   .��59      � ���#1a1a1a.+�5;      �� -   5         ��Name$ logs_richtextbox��5;   "   $��Stretch=�6<   $��0,0,0,0q�6Z   $��Stretch=�6k    �� 
IsReadOnly . $��16(�5<   "   ���#1a1a1a
�������. �5=      /� .T�5>      J� .*�6(   .*�5?   "   �� $� ��6&   5@      5A      5B      5D      5E      5I      5J      5K      