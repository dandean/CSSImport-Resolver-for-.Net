<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" %>
<!DOCTYPE html>
<html>
	<head runat="server">
		<title>CSSImport Test Website</title>
		<link rel="stylesheet" media="all" type="text/css" href="css/main.css">
	</head>
	<body>
		<form id="form1" runat="server">
			<p>If everything works correctly:</p>
			<ul>
				<li>Background should be red</li>
				<li>Color should be white</li>
				<li>Font should be Courier or Mono</li>
				<li><a href="#">Links</a> should be line-through</li>
			</ul>
			
			<ul id="images">
				<li></li>
				<li></li>
				<li></li>
				<li></li>
			</ul>
		</form>
	</body>
</html>
