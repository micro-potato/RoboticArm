package com {
	
	import flash.display.MovieClip;
	import flash.external.ExternalInterface;
	import flash.display.Sprite;
	import flash.text.TextField;
	import flash.events.MouseEvent;
	import flash.events.Event;
	
	public class Main extends MovieClip {
		
		var _initY:Number=186.3;
		public function Main() {
			// constructor code
			if(ExternalInterface.available)
			{
				ExternalInterface.addCallback("datain",OnDataIn);
			}
			//Test
			stage.addEventListener(MouseEvent.CLICK,OnTest);
		}
		
		function OnTest(e:Event)
		{
			OnDataIn("-7.45,-62.81,-174.91,-15.12,-39.35,-178.61");
			//OnDataIn("72.27,0.93,144.95,33.3,47.4,64.03");
			//this["y2co"].rotation=0.0;
			//this["y2co"].height=50;
		}
		
		function OnDataIn(data:String)
		{
			trace("data in:"+data);
			if(!(data&&data!=""))
			{
				trace("over");
				return;
			}
			var dataParts:Array=data.split(',');
			var r1:String=dataParts[0];
			var p1:String=dataParts[1];
			var y1:String=dataParts[2];
			var r2:String=dataParts[3];
			var p2:String=dataParts[4];
			var y2:String=dataParts[5];
			UpdateData("r1",Number(r1));
			UpdateData("p1",Number(p1));
			UpdateData("y1",Number(y1));
			UpdateData("r2",Number(r2));
			UpdateData("p2",Number(p2));
			UpdateData("y2",Number(y2));
			
		}
		
		function UpdateData(dataName:String,dataValue:Number)
		{
			trace("update:"+dataName+"	"+dataValue);
			var dataColumn:MovieClip=this[dataName+"co"] as MovieClip;
			var dataText:TextField=this[dataName+"Text"] as TextField;
			if(dataName=="y1"||dataName=="y2")
			{
				
			}
			if(dataValue>=0)
			{
				dataColumn.rotation=-180;
				dataColumn.height=dataValue;
			}
			else
			{
				dataColumn.rotation=0.0;
				dataColumn.height=dataValue*(-1);
			}
			dataText.text=dataValue.toString();
			trace(dataName+" height:"+dataColumn.height);
		}
	}
	
}
