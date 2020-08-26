import React from 'react'
import Paper from '@material-ui/core/Paper';
import {
    Chart,
    PieSeries,
    Title,
    Legend
} from '@devexpress/dx-react-chart-material-ui';
import { ThemeProvider } from '@material-ui/styles'
import theme from './theme.js'

const data = [
    { country: 'Russia', area: 12 },
    { country: 'Canada', area: 7 },
    { country: 'USA', area: 7 },
    { country: 'China', area: 7 },
    { country: 'Brazil', area: 6 },
    { country: 'Australia', area: 5 },
    { country: 'India', area: 2 },
    { country: 'Others', area: 55 },
];

class ConsumptionDevices extends React.Component {

    constructor(props) {
        super(props);
    
        this.state = {
          data,
        };
    }

    componentDidMount(){

    }

      render() {
        const { data: chartData } = this.state;
    
        return (
            <ThemeProvider theme={theme}>
                <Paper>
                    <Chart
                    data={chartData}
                    >
                        <PieSeries
                            valueField="area"
                            argumentField="country"
                        />
                        <Title text="Monthly Usage Of Water" />
                    </Chart>
                </Paper>
            </ThemeProvider>
        );
      }
}

export default ConsumptionDevices;

   {/* width='300' height='300' */}