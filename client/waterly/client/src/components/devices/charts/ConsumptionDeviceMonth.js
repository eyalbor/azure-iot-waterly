import * as React from 'react';
import Paper from '@material-ui/core/Paper';
import {
  Chart,
  BarSeries,
  LineSeries,
  ArgumentAxis,
  ValueAxis,
  Title,
  Legend,
} from '@devexpress/dx-react-chart-material-ui';

import { ValueScale, Stack } from '@devexpress/dx-react-chart';

export const oilProduction = [
    {
      year: '1970', saudiArabia: 241.142, usa: 482.150, iran: 230.174, mexico: 23.640, price: 17, consumption: 570,
    }, {
      year: '1980', saudiArabia: 511.334, usa: 437.343, iran: 75.097, mexico: 108.249, price: 104, consumption: 630,
    }, {
      year: '1990', saudiArabia: 324.359, usa: 374.867, iran: 165.284, mexico: 141.060, russia: 516.040, price: 23.7, consumption: 590,
    }, {
      year: '2000', saudiArabia: 410.060, usa: 297.513, iran: 196.877, mexico: 159.630, russia: 312.821, price: 28.3, consumption: 680,
    }, {
      year: '2010', saudiArabia: 413.505, usa: 279.225, iran: 200.318, mexico: 144.975, russia: 487.106, price: 79.6, consumption: 640,
    }, {
      year: '2015', saudiArabia: 516.157, usa: 437.966, iran: 142.087, mexico: 121.587, russia: 512.777, price: 52.4, consumption: 790,
    },
  ];

const Label = symbol => (props) => {
  const { text } = props;
  return (
    <ValueAxis.Label
      {...props}
      text={text + symbol}
    />
  );
};

const PriceLabel = Label(' $');
const LabelWithThousand = Label(' k');

const modifyOilDomain = domain => [domain[0], 2200];
const modifyPriceDomain = () => [0, 110];

export default class ConsumptionDeviceMonth extends React.PureComponent {
  constructor(props) {
    super(props);

    this.state = {
      oilProduction,
    };
  }

  render() {
    const { oilProduction: chartData } = this.state;

    return (
      <Paper>
        <Chart
          data={chartData}
        >
          <ValueScale name="oil" modifyDomain={modifyOilDomain} />
          <ValueScale name="price" modifyDomain={modifyPriceDomain} />

          <ArgumentAxis />
          <ValueAxis
            scaleName="oil"
            labelComponent={LabelWithThousand}
          />
          <ValueAxis
            scaleName="price"
            position="right"
            labelComponent={PriceLabel}
          />

          <Title
            text="Oil production vs Oil price"
          />

          <BarSeries
            name="USA"
            valueField="usa"
            argumentField="year"
            scaleName="oil"
          />
          <BarSeries
            name="Saudi Arabia"
            valueField="saudiArabia"
            argumentField="year"
            scaleName="oil"
          />
          <BarSeries
            name="Iran"
            valueField="iran"
            argumentField="year"
            scaleName="oil"
          />
          <BarSeries
            name="Mexico"
            valueField="mexico"
            argumentField="year"
            scaleName="oil"
          />
          <BarSeries
            name="Russia"
            valueField="russia"
            argumentField="year"
            scaleName="oil"
          />
          <LineSeries
            name="Oil Price"
            valueField="price"
            argumentField="year"
            scaleName="price"
          />
          <Stack
            stacks={[
              { series: ['USA', 'Saudi Arabia', 'Iran', 'Mexico', 'Russia'] },
            ]}
          />
          <Legend />
        </Chart>
      </Paper>
    );
  }
}
