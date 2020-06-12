import React from 'react'
import { connect } from 'react-redux'
import { fetchBills } from '../../actions'

import Paper from '@material-ui/core/Paper';
import {
    Chart,
    ArgumentAxis,
    ValueAxis,
    BarSeries,
    Title,
    Legend,
  } from '@devexpress/dx-react-chart-material-ui';
import { Stack, Animation } from '@devexpress/dx-react-chart';

import { withStyles } from '@material-ui/core/styles';

const legendStyles = () => ({
    root: {
        display: 'flex',
        margin: 'auto',
        flexDirection: 'row',
    },
});

const legendRootBase = ({ classes, ...restProps }) => (
    <Legend.Root {...restProps} className={classes.root} />
);
const Root = withStyles(legendStyles, { name: 'LegendRoot' })(legendRootBase);
const legendLabelStyles = () => ({
    label: {
        whiteSpace: 'nowrap',
}   ,
});
const legendLabelBase = ({ classes, ...restProps }) => (
    <Legend.Label className={classes.label} {...restProps} />
);
const Label = withStyles(legendLabelStyles, { name: 'LegendLabel' })(legendLabelBase);

class BillShow extends React.Component {

    componentDidMount(){
        this.props.fetchBills();
    }

    renderCards(){
        return this.props.bills.map(bill => {
            return (      
                <div className="ui card" key={bill.id} style={{padding: "20px", margin:"20px"}}>
                    <div className="content">
                        <div className="header">Month {bill.time.month}/{bill.time.year}</div>
                    </div>
                    <div className="content">
                        <h4 className="ui sub header">{bill.money.total_price} ILS</h4>
                        <div className="ui small feed">
                            <div className="event">
                                <div className="content">
                                <div className="summary">
                                    Total flow for this week is: <a>{bill.total_flow} m³/h</a>
                                </div>
                                </div>
                            </div>
                            <div className="event">
                                <div className="content">
                                <div className="summary">
                                    Water expenses: <a>{bill.money.water_expenses} ILS</a>
                                    <br/>
                                    Fixed expenses: <a>{bill.money.fixed_expenses} ILS</a>
                                </div>
                                </div>
                            </div>
                            <div className="event">
                                <div className="content">
                                <div className="summary">
                                    <Paper>
                                        <Chart data={[{month:`${bill.time.month}`, user: bill.total_flow , avg: 100}]}>
                                            <ArgumentAxis/>
                                            <ValueAxis/>
                                            <BarSeries name="User" valueField="user" argumentField="month" color="#ffd700"/>
                                            <BarSeries name="Avg" valueField="avg" argumentField="month" color="#c0c0c0"/>
                                            <Animation />
                                            <Legend position="bottom" rootComponent={Root} labelComponent={Label} />
                                            <Title text="Monthly Usage Of Water (m³/h)" />
                                            <Stack />
                                        </Chart>
                                    </Paper>
                                </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div className="extra content">
                        <button className="ui primary button">Pay</button>
                    </div>
                </div>
            )
        })
    }

    render(){
        return (
            <div className="ui container">
                <div className="ui special cards">
                    {this.renderCards()}
                </div>
            </div>
        )
    }
  
};

const mapStateToProps = (state) => {
    return {
        bills: Object.values(state.bills),
        currentUserId: state.auth.userId,
        isSignedIn: state.auth.isSignedIn
    }
}

export default connect(mapStateToProps, {fetchBills})(BillShow);