import * as React from 'react';
import Paper from '@material-ui/core/Paper';
import Chart, {
  CommonSeriesSettings,
  Series,
  ValueAxis,
  Export,
  Legend,
  Tooltip,
  Title,
  Grid,
  Format
} from 'devextreme-react/chart';
import _ from 'lodash'

import { consumptionForYearEachMonth } from '../../../actions/index'

// export const devicesSources = [
//   { value: 'WaterlyIotDevice1', name: 'WaterlyIotDevice2' },
//   { value: 'WaterlyIotDevice2', name: 'WaterlyIotDevice2' },
//   { value: 'WaterlyIotDevice3', name: 'WaterlyIotDevice3' },
//   { value: 'WaterlyIotDevice4', name: 'WaterlyIotDevice4' },
//   { value: 'WaterlyIotDevice5', name: 'WaterlyIotDevice5' },
//   { value: 'WaterlyIotDevice6', name: 'WaterlyIotDevice6' },
//   { value: 'WaterlyIotDevice7', name: 'WaterlyIotDevice7' },
// ];

export default class ConsumptionDeviceMonth extends React.PureComponent {
  constructor(props) {
    super(props);
    console.log("props" + props)
    this.state = {data: null, devices: null}
  }

  prepareDataForChart(data){
    this.setState({data})
    console.log(data)
    const devicesSources = new Set();
    const copyData = JSON.parse(JSON.stringify(data))
    copyData.map(obj => {
      delete obj.Month;
      delete obj.Average;
      let tuple = JSON.stringify(obj).split(',')
      tuple.map(obj2 => {
        //console.log(obj2)
        let array_tuple = (obj2).split(':')
        console.log(array_tuple[0].replace('{',''))
        devicesSources.add(JSON.parse(array_tuple[0].replace('{','')))
      })
      //let arr_tuple = JSON.stringify(tuple).split(':')
      //console.log(JSON.stringify(arr_tuple[0]))
    })
      //devicesSources.add({value: , name:})
      console.log(devicesSources)
      this.setState({devices: Array.from(devicesSources)})

  }

  componentDidMount(){
    consumptionForYearEachMonth(this.props.userId)
    .then(res=>{
      //console.log(res.data)
      this.prepareDataForChart(res.data)
    })
    .catch(err => {
      console.log(err)
    })
  }

  render() {
    if(this.state.devices != null){
      console.log(this.state)
      return (
        <Paper>
        <Chart
          id="chartConsumption"
          palette="Soft"
          dataSource={this.state.data}
          ignoreEmptyPoints={true}
        >
          <CommonSeriesSettings
            argumentField="Month"
            type="stackedBar"
          />
          {
            this.state.devices.map(function(item) {
              return <Series key={item} valueField={item} name={item} />;
            })
          }
          <Series
            axis="Average"
            type="spline"
            valueField="Average"
            name="Average"
            color="#008fd8"
          />
  
          <ValueAxis>
            
          </ValueAxis>
          <ValueAxis
            name="Average"
            position="right"
            title="Average Consumption, m^3/s"
          >
          <Grid visible={true} />
          </ValueAxis>
          <Legend
            verticalAlignment="bottom"
            horizontalAlignment="center"
          />
          <Export enabled={true} />
          <Format
            type="largeNumber"
            precision={1}
          />
          <Tooltip
            enabled={true}
            location="edge"
          />
          <Title text="Your Monthly Water Consumption" />
        </Chart>
        </Paper>
      );
    }
    else {
      return <div class="ui active centered inline loader"></div>
    }
  }
}
