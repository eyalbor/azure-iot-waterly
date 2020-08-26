import * as React from 'react';
import Paper from '@material-ui/core/Paper';
import {
  Chart,
  Series,
  ValueAxis,
  ArgumentAxis,
  Title,
  CommonSeriesSettings,
  Legend,
  Aggregation,
  Tooltip,
  ConstantLine,
  Label
} from 'devextreme-react/chart';

const ScatterEvents= ({data}) => {
    const dataGen = []
    data.map(event => {
      dataGen.push({
        arg1: event.ph,
        val1: event.pressure,
      });
    })

    function customizeTooltip(pointInfo) {
      //console.log(pointInfo.point)
      let text1 = `[${pointInfo.argument.toFixed(1)}, ${pointInfo.originalValue.toFixed(1)}]`
      let text2 =""
      if(pointInfo.point.aggregationInfo != null){
         text2 = `${pointInfo.point.aggregationInfo.data.length} points`
      }
      return <div>{text1}<br /> {text2}</div> 
    }

    return (
      <Paper>
        <Chart id="scatterChart" dataSource={dataGen}>
          <CommonSeriesSettings type="scatter" />
          <Title text="Ph vs Pressure" />
          <ArgumentAxis title="ph" tickInterval={1} visualRange={[0,14]}>
          <ConstantLine
            value={6.9}
            width={2}
            color="#fc3535"
            dashStyle="dash"
          >
            <Label visible={false} />
          </ConstantLine>

          <ConstantLine
            value={7.1}
            width={2}
            color="#fc3535"
            dashStyle="dash"
          >
            <Label visible={false} />
          </ConstantLine>

            </ArgumentAxis>
          <ValueAxis title="pressure [atm]" tickInterval={1} visualRange={[0,5]}>

          <ConstantLine
            value={1.2}
            width={2}
            color="#fc3535"
            dashStyle="dash"
          >
            <Label visible={false} />
          </ConstantLine>

          <ConstantLine
            value={2.5}
            width={2}
            color="#fc3535"
            dashStyle="dash"
          >
            <Label visible={false} />
          </ConstantLine>

          </ValueAxis>
            <Series
              valueField="val1"
              argumentField="arg1"
              type=""
            >
            <Aggregation enabled={true} />
            </Series>
            <Series
              valueField="val2"
              argumentField="arg2"
            />
            <Tooltip
              enabled={true}
              contentRender={customizeTooltip}
            />
            <Legend visible={false} />
        </Chart>
      </Paper>
    );
}

export default ScatterEvents;
