import React from 'react'
import Paper from '@material-ui/core/Paper';
import {
    Chart,
    BarSeries,
    Title,
    ValueAxis,
    ArgumentAxis
  } from '@devexpress/dx-react-chart-material-ui';
import { ThemeProvider } from '@material-ui/styles'
import { scaleBand } from '@devexpress/dx-chart-core';
import { ArgumentScale } from '@devexpress/dx-react-chart';

import theme from './theme.js'

const data = [
    { year: '1910', population: 2.525 },
    { year: '1920', population: 3.018 },
    { year: '1930', population: 3.682 },
    { year: '1940', population: 4.440 },
    { year: '1950', population: 5.310 },
    { year: '1960', population: 6.127 },
    { year: '1970', population: 6.930 },
    { year: '1980', population: 3.682 },
    { year: '1990', population: 4.440 },
    { year: '2000', population: 6.127 },
  ];

class ConsumptionPerMonth extends React.Component {

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
                        <ArgumentAxis />
                        <ValueAxis />
                        <ArgumentScale factory={scaleBand} />
                        <BarSeries
                            valueField="population"
                            argumentField="year"
                        />
                        <Title text="Yearly Usage Of Water Per Month" />
                    </Chart>
                </Paper>
            </ThemeProvider>
        );
        }
}

export default ConsumptionPerMonth;